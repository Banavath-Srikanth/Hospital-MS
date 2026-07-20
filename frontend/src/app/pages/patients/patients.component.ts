import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PatientService, Patient, CreatePatientDto, UpdatePatientDto } from '../../services/patient';
import { ExportService } from '../../services/export.service';
import * as XLSX from 'xlsx';
import { GridModule, PageChangeEvent, GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';

@Component({
  selector: 'app-patients',
  standalone: true,
  imports: [CommonModule, FormsModule, GridModule],
  templateUrl: './patients.component.html',
  styleUrl: './patients.component.scss'
})
export class PatientsComponent implements OnInit {
  patients: Patient[] = [];
  filtered: Patient[] = [];

  // Global search
  globalSearch = '';
  statusFilter: 'All' | 'Active' | 'Inactive' = 'All';

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

  form = this.emptyForm();

  constructor(
    private svc: PatientService,
    private cdr: ChangeDetectorRef,
    private exportSvc: ExportService
  ) {}

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.error = '';
    this.svc.getAll().subscribe({
      next: data => {
        this.patients = data;
        this.applyFilter();
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: err => {
        this.error = 'Failed to load patients. Is the backend running?';
        this.loading = false;
        this.cdr.markForCheck();
        console.error(err);
      }
    });
  }

  applyFilter() {
    const q = this.globalSearch.toLowerCase().trim();
    this.filtered = this.patients.filter(p => {
      const matchStatus =
        this.statusFilter === 'All' ||
        (this.statusFilter === 'Active'   &&  p.isActive) ||
        (this.statusFilter === 'Inactive' && !p.isActive);
      if (!q) return matchStatus;
      return matchStatus && (
        p.fullName.toLowerCase().includes(q) ||
        p.gender.toLowerCase().includes(q) ||
        p.disease.toLowerCase().includes(q) ||
        p.phoneNumber.toLowerCase().includes(q) ||
        String(p.age).includes(q) ||
        (p.isActive ? 'active' : 'inactive').includes(q)
      );
    });
    this.skip = 0;
    this.updateGridView();
  }

  setStatusFilter(f: 'All' | 'Active' | 'Inactive') {
    this.statusFilter = f;
    this.applyFilter();
  }

  private get exportColumns() {
    return [
      { header: '#',         key: 'id',           width: 10 },
      { header: 'Full Name', key: 'fullName',      width: 36 },
      { header: 'Age',       key: 'age',           width: 12 },
      { header: 'Gender',    key: 'gender',        width: 16 },
      { header: 'Disease',   key: 'disease',       width: 36 },
      { header: 'Phone',     key: 'phoneNumber',   width: 24 },
      { header: 'Admitted',  key: 'admitted',      width: 24 },
      { header: 'Status',    key: 'status',        width: 16 },
      { header: 'Appts',     key: 'appts',         width: 12 },
    ];
  }

  private get exportRows() {
    return this.filtered.map(p => ({
      id:          p.id,
      fullName:    p.fullName,
      age:         p.age,
      gender:      p.gender,
      disease:     p.disease,
      phoneNumber: p.phoneNumber,
      admitted:    p.admissionDate ? new Date(p.admissionDate).toLocaleDateString('en-GB') : '',
      status:      p.isActive ? 'Active' : 'Inactive',
      appts:       p.totalAppointments,
    }));
  }

  exportToExcel() {
    const rows = this.filtered.map(p => ({
      'ID':             p.id,
      'Full Name':      p.fullName,
      'Age':            p.age,
      'Gender':         p.gender,
      'Disease':        p.disease,
      'Phone':          p.phoneNumber,
      'Admitted':       p.admissionDate ? new Date(p.admissionDate).toLocaleDateString('en-GB') : '',
      'Status':         p.isActive ? 'Active' : 'Inactive',
      'Appointments':   p.totalAppointments
    }));
    const ws = XLSX.utils.json_to_sheet(rows);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Patients');
    XLSX.writeFile(wb, `patients_${new Date().toISOString().slice(0,10)}.xlsx`);
  }

  exportToPdf() {
    this.exportSvc.exportToPdf(
      'Patients Report',
      `Total: ${this.filtered.length} patient(s)  |  Status: ${this.statusFilter}`,
      this.exportColumns,
      this.exportRows,
      `patients_${new Date().toISOString().slice(0,10)}.pdf`
    );
  }

  printData() {
    this.exportSvc.printTable(
      'Patients Report',
      `Total: ${this.filtered.length} patient(s)  |  Status: ${this.statusFilter}`,
      this.exportColumns,
      this.exportRows
    );
  }

  clearFilters() {
    this.globalSearch = '';
    this.statusFilter = 'All';
    this.sort = [];
    this.skip = 0;
    this.applyFilter();
  }

  openAdd() {
    this.form = this.emptyForm();
    this.editMode   = false;
    this.phoneError = '';
    this.showModal  = true;
  }

  openEdit(p: Patient) {
    this.form = {
      id:          p.id,
      fullName:    p.fullName,
      age:         p.age,
      gender:      p.gender,
      disease:     p.disease,
      phoneNumber: p.phoneNumber,
      isActive:    p.isActive
    };
    this.editMode   = true;
    this.phoneError = '';
    this.showModal  = true;
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

  save() {
    if (!this.validatePhone()) return;
    if (this.editMode) {
      const dto: UpdatePatientDto = {
        fullName:    this.form.fullName,
        age:         this.form.age,
        gender:      this.form.gender,
        disease:     this.form.disease,
        phoneNumber: this.form.phoneNumber,
        isActive:    this.form.isActive
      };
      this.svc.update(this.form.id, dto).subscribe({
        next: () => { this.showModal = false; this.flash('Patient updated successfully.'); this.load(); },
        error: err => { this.error = err?.error?.message || 'Update failed.'; }
      });
    } else {
      const dto: CreatePatientDto = {
        fullName:    this.form.fullName,
        age:         this.form.age,
        gender:      this.form.gender,
        disease:     this.form.disease,
        phoneNumber: this.form.phoneNumber
      };
      this.svc.create(dto).subscribe({
        next: () => { this.showModal = false; this.flash('Patient registered successfully.'); this.load(); },
        error: err => { this.error = err?.error?.message || 'Create failed.'; }
      });
    }
  }

  delete(id: number) {
    if (!confirm('Deactivate this patient?')) return;
    this.svc.delete(id).subscribe({
      next: () => { this.flash('Patient deactivated.'); this.load(); },
      error: err => { this.error = err?.error?.message || 'Delete failed.'; }
    });
  }

  private emptyForm() {
    return { id: 0, fullName: '', age: 0, gender: 'Male', disease: '', phoneNumber: '', isActive: true };
  }

  private flash(msg: string) {
    this.success = msg;
    setTimeout(() => this.success = '', 3000);
  }
}
