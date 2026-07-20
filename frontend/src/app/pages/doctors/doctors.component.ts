import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  DoctorService, Doctor, Department,
  CreateDoctorDto, UpdateDoctorDto
} from '../../services/doctor';
import { ExportService } from '../../services/export.service';
import * as XLSX from 'xlsx';
import { GridModule, PageChangeEvent, GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';

@Component({
  selector: 'app-doctors',
  standalone: true,
  imports: [CommonModule, FormsModule, GridModule],
  templateUrl: './doctors.component.html',
  styleUrl: './doctors.component.scss'
})
export class DoctorsComponent implements OnInit {
  doctors: Doctor[] = [];
  filtered: Doctor[] = [];
  departments: Department[] = [];

  // Global search
  globalSearch      = '';
  statusFilter: 'All' | 'Available' | 'Unavailable' = 'All';

  // ── Kendo Grid Pagination ──────────────────────────────────────
  pageSize = 10;
  skip     = 0;
  gridView: GridDataResult = { data: [], total: 0 };

  pageableSettings = {
    buttonCount: 5,
    pageSizes: [5, 10, 20, 50],
    info: true,
    type: 'numeric' as const,
    previousNext: true
  };

  // ── Kendo Grid Sorting ─────────────────────────────────────────
  sort: SortDescriptor[] = [];

  private updateGridView() {
    const sorted = this.sort.length ? orderBy(this.filtered, this.sort) : this.filtered;
    this.gridView = {
      data: sorted.slice(this.skip, this.skip + this.pageSize),
      total: this.filtered.length
    };
  }

  onPageChange(event: PageChangeEvent) {
    this.skip = event.skip;
    this.pageSize = event.take;
    this.updateGridView();
  }

  onSortChange(sort: SortDescriptor[]) {
    this.sort = sort;
    this.skip = 0;
    this.updateGridView();
  }
  // ──────────────────────────────────────────────────────────────

  showModal = false;
  editMode  = false;
  loading   = false;
  error     = '';
  success   = '';
  phoneError = '';
  emailError = '';
  departmentError = '';

  form = this.emptyForm();

  constructor(
    private svc: DoctorService,
    private cdr: ChangeDetectorRef,
    private exportSvc: ExportService
  ) {}

  ngOnInit() {
    this.load();
    this.svc.getDepartments().subscribe({
      next: data => { this.departments = data; this.cdr.markForCheck(); },
      error: () => {}
    });
  }

  load() {
    this.loading = true;
    this.error = '';
    this.svc.getAll().subscribe({
      next: data => {
        this.doctors = data;
        this.applyFilter();
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: err => {
        this.error = 'Failed to load doctors. Is the backend running?';
        this.loading = false;
        this.cdr.markForCheck();
        console.error(err);
      }
    });
  }

  applyFilter() {
    const q = this.globalSearch.toLowerCase().trim();
    this.filtered = this.doctors.filter(d => {
      const matchStatus =
        this.statusFilter === 'All' ||
        (this.statusFilter === 'Available'   &&  d.isAvailable) ||
        (this.statusFilter === 'Unavailable' && !d.isAvailable);
      if (!q) return matchStatus;
      return matchStatus && (
        d.fullName.toLowerCase().includes(q) ||
        d.specialization.toLowerCase().includes(q) ||
        (d.payrollPosition || '').toLowerCase().includes(q) ||
        (d.departmentName || '').toLowerCase().includes(q) ||
        d.badgeId.toLowerCase().includes(q) ||
        d.phoneNumber.toLowerCase().includes(q) ||
        d.email.toLowerCase().includes(q)
      );
    });
    this.skip = 0;
    this.updateGridView();
  }

  setStatusFilter(f: 'All' | 'Available' | 'Unavailable') {
    this.statusFilter = f;
    this.applyFilter();
  }

  private get exportColumns() {
    return [
      { header: 'Badge ID',       key: 'badgeId',        width: 22 },
      { header: 'Full Name',      key: 'fullName',       width: 36 },
      { header: 'Specialization', key: 'specialization', width: 30 },
      { header: 'Position',       key: 'position',       width: 28 },
      { header: 'Department',     key: 'department',     width: 28 },
      { header: 'Phone',          key: 'phoneNumber',    width: 22 },
      { header: 'Email',          key: 'email',          width: 36 },
      { header: 'Status',         key: 'status',         width: 20 },
      { header: 'Appts',          key: 'appts',          width: 12 },
    ];
  }

  private get exportRows() {
    return this.filtered.map(d => ({
      badgeId:        d.badgeId,
      fullName:       'Dr. ' + d.fullName,
      specialization: d.specialization,
      position:       d.payrollPosition || '',
      department:     d.departmentName  || '',
      phoneNumber:    d.phoneNumber,
      email:          d.email,
      status:         d.isAvailable ? 'Available' : 'Unavailable',
      appts:          d.totalAppointments,
    }));
  }

  exportToExcel() {
    const rows = this.filtered.map(d => ({
      'Badge ID':       d.badgeId,
      'Full Name':      'Dr. ' + d.fullName,
      'Specialization': d.specialization,
      'Position':       d.payrollPosition || '',
      'Department':     d.departmentName || '',
      'Phone':          d.phoneNumber,
      'Email':          d.email,
      'Status':         d.isAvailable ? 'Available' : 'Unavailable',
      'Appointments':   d.totalAppointments
    }));
    const ws = XLSX.utils.json_to_sheet(rows);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Doctors');
    XLSX.writeFile(wb, `doctors_${new Date().toISOString().slice(0, 10)}.xlsx`);
  }

  exportToPdf() {
    this.exportSvc.exportToPdf(
      'Doctors / Employees Report',
      `Total: ${this.filtered.length} doctor(s)  |  Status: ${this.statusFilter}`,
      this.exportColumns,
      this.exportRows,
      `doctors_${new Date().toISOString().slice(0,10)}.pdf`
    );
  }

  printData() {
    this.exportSvc.printTable(
      'Doctors / Employees Report',
      `Total: ${this.filtered.length} doctor(s)  |  Status: ${this.statusFilter}`,
      this.exportColumns,
      this.exportRows
    );
  }

  clearFilters() {
    this.globalSearch = '';
    this.statusFilter = 'All';
    this.applyFilter();
  }

  openAdd() {
    this.form = this.emptyForm();
    this.editMode  = false;
    this.phoneError = '';
    this.emailError = '';
    this.departmentError = '';
    this.showModal = true;
  }

  openEdit(d: Doctor) {
    this.form = {
      id:              d.id,
      badgeId:         d.badgeId,
      fullName:        d.fullName,
      specialization:  d.specialization,
      payrollPosition: d.payrollPosition,
      phoneNumber:     d.phoneNumber,
      email:           d.email,
      isAvailable:     d.isAvailable,
      departmentId:    d.departmentId
    };
    this.editMode  = true;
    this.phoneError = '';
    this.emailError = '';
    this.departmentError = '';
    this.showModal = true;
  }

  validatePhone(): boolean {
    const phone = this.form.phoneNumber?.trim() || '';
    const phoneRegex = /^[6-9]\d{9}$/;
    if (!phone) { this.phoneError = 'Phone number is required.'; return false; }
    if (!phoneRegex.test(phone)) {
      this.phoneError = 'Phone must be 10 digits and start with 6, 7, 8, or 9.';
      return false;
    }
    this.phoneError = '';
    return true;
  }

  validateEmail(): boolean {
    const email = this.form.email?.trim() || '';
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!email) { this.emailError = 'Email is required.'; return false; }
    if (!emailRegex.test(email)) {
      this.emailError = 'Please enter a valid email address.';
      return false;
    }
    this.emailError = '';
    return true;
  }

  validateDepartment(): boolean {
    if (!this.form.departmentId) {
      this.departmentError = 'Department is required.';
      return false;
    }
    this.departmentError = '';
    return true;
  }

  save() {
    const phoneOk = this.validatePhone();
    const emailOk = this.validateEmail();
    const deptOk  = this.validateDepartment();
    if (!phoneOk || !emailOk || !deptOk) return;

    if (this.editMode) {
      const dto: UpdateDoctorDto = {
        fullName:        this.form.fullName,
        specialization:  this.form.specialization,
        payrollPosition: this.form.payrollPosition,
        phoneNumber:     this.form.phoneNumber,
        email:           this.form.email,
        isAvailable:     this.form.isAvailable,
        departmentId:    this.form.departmentId!
      };
      this.svc.update(this.form.id, dto).subscribe({
        next: () => { this.showModal = false; this.flash('Doctor updated successfully.'); this.load(); },
        error: err => { this.error = err?.error?.message || 'Update failed.'; }
      });
    } else {
      const dto: CreateDoctorDto = {
        fullName:        this.form.fullName,
        specialization:  this.form.specialization,
        payrollPosition: this.form.payrollPosition,
        phoneNumber:     this.form.phoneNumber,
        email:           this.form.email,
        departmentId:    this.form.departmentId!
      };
      this.svc.create(dto).subscribe({
        next: () => { this.showModal = false; this.flash('Doctor registered successfully.'); this.load(); },
        error: err => { this.error = err?.error?.message || 'Create failed.'; }
      });
    }
  }

  delete(id: number) {
    if (!confirm('Remove this doctor from the system?')) return;
    this.svc.delete(id).subscribe({
      next: () => { this.flash('Doctor removed.'); this.load(); },
      error: err => { this.error = err?.error?.message || 'Delete failed.'; }
    });
  }

  private emptyForm() {
    return {
      id: 0, badgeId: '', fullName: '', specialization: '', payrollPosition: '',
      phoneNumber: '', email: '', isAvailable: true, departmentId: null as number | null
    };
  }

  private flash(msg: string) {
    this.success = msg;
    setTimeout(() => this.success = '', 3000);
  }
}
