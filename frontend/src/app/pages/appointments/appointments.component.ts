import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppointmentService, Appointment, CreateAppointmentDto, UpdateAppointmentDto } from '../../services/appointment';
import { PatientService, Patient } from '../../services/patient';
import { DoctorService, Doctor } from '../../services/doctor';
import { ExportService } from '../../services/export.service';
import { GridModule, PageChangeEvent, GridDataResult } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule, GridModule],
  templateUrl: './appointments.component.html',
  styleUrl: './appointments.component.scss'
})
export class AppointmentsComponent implements OnInit {
  appointments: Appointment[] = [];
  filtered: Appointment[] = [];
  patients: Patient[] = [];
  doctors: Doctor[] = [];
  searchTerm = '';
  statusFilter: 'All' | 'Scheduled' | 'Cancelled' | 'Completed' = 'All';
  dateFrom = '';
  dateTo   = '';
  showModal = false;
  editMode = false;
  loading = false;
  error = '';
  success = '';
  minDateTime = '';

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

  form = this.emptyForm();

  constructor(
    private svc: AppointmentService,
    private patientSvc: PatientService,
    private doctorSvc: DoctorService,
    private cdr: ChangeDetectorRef,
    private exportSvc: ExportService
  ) {}

  ngOnInit() {
    this.load();
    this.patientSvc.getAll().subscribe({ next: p => { this.patients = p; this.cdr.markForCheck(); } });
    this.doctorSvc.getAll().subscribe({ next: d => { this.doctors = d; this.cdr.markForCheck(); } });
  }

  load() {
    this.loading = true;
    this.error = '';
    this.svc.getAll().subscribe({
      next: data => {
        this.appointments = data;
        this.applyFilter();
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: err => {
        this.error = 'Failed to load appointments. Is the backend running?';
        this.loading = false;
        this.cdr.markForCheck();
        console.error(err);
      }
    });
  }

  applyFilter() {
    const q = this.searchTerm.toLowerCase();
    const from = this.dateFrom ? new Date(this.dateFrom + 'T00:00:00').getTime() : null;
    const to   = this.dateTo   ? new Date(this.dateTo   + 'T23:59:59').getTime() : null;

    this.filtered = this.appointments.filter(a => {
      const matchStatus = this.statusFilter === 'All' || a.status === this.statusFilter;
      const apptTime = new Date(a.appointmentDate).getTime();
      const matchFrom = from === null || apptTime >= from;
      const matchTo   = to   === null || apptTime <= to;
      const matchDate = matchFrom && matchTo;
      if (!q) return matchStatus && matchDate;
      return matchStatus && matchDate &&
        `${a.patientName} ${a.doctorName} ${a.status} ${a.appointmentDate}`.toLowerCase().includes(q);
    });
    this.skip = 0;
    this.updateGridView();
  }

  private get exportColumns() {
    return [
      { header: '#',             key: 'id',               width: 10 },
      { header: 'Patient',       key: 'patientName',      width: 36 },
      { header: 'Doctor',        key: 'doctorName',       width: 36 },
      { header: 'Specialization',key: 'specialization',   width: 30 },
      { header: 'Date & Time',   key: 'appointmentDate',  width: 36 },
      { header: 'Status',        key: 'status',           width: 20 },
    ];
  }

  private get exportRows() {
    return this.filtered.map(a => ({
      id:             a.id,
      patientName:    a.patientName,
      doctorName:     'Dr. ' + a.doctorName,
      specialization: a.doctorSpecialization,
      appointmentDate: new Date(a.appointmentDate).toLocaleString('en-IN', {
        day: '2-digit', month: 'short', year: 'numeric',
        hour: '2-digit', minute: '2-digit'
      }),
      status: a.status,
    }));
  }

  exportToPdf() {
    this.exportSvc.exportToPdf(
      'Appointments Report',
      `Total: ${this.filtered.length} appointment(s)  |  Status: ${this.statusFilter}`,
      this.exportColumns,
      this.exportRows,
      `appointments_${new Date().toISOString().slice(0,10)}.pdf`
    );
  }

  printData() {
    this.exportSvc.printTable(
      'Appointments Report',
      `Total: ${this.filtered.length} appointment(s)  |  Status: ${this.statusFilter}`,
      this.exportColumns,
      this.exportRows
    );
  }

  setStatusFilter(f: 'All' | 'Scheduled' | 'Cancelled' | 'Completed') {
    this.statusFilter = f;
    this.applyFilter();
  }

  clearFilters() {
    this.searchTerm  = '';
    this.statusFilter = 'All';
    this.dateFrom    = '';
    this.dateTo      = '';
    this.sort        = [];
    this.applyFilter();
  }

  get hasActiveFilters(): boolean {
    return !!this.searchTerm || this.statusFilter !== 'All' || !!this.dateFrom || !!this.dateTo;
  }

  openAdd() {
    this.minDateTime = this.nowLocalIso();
    this.form = this.emptyForm();
    this.editMode = false;
    this.showModal = true;
  }

  openEdit(a: Appointment) {
    this.minDateTime = this.nowLocalIso();
    this.form = {
      id: a.id,
      patientId: a.patientId,
      doctorId: a.doctorId,
      appointmentDate: a.appointmentDate.slice(0, 16),
      status: a.status
    };
    this.editMode = true;
    this.showModal = true;
  }

  save() {
    if (!this.form.appointmentDate) {
      this.error = 'Please select a date and time.';
      return;
    }
    if (new Date(this.form.appointmentDate) <= new Date()) {
      this.error = 'Appointment date and time must be in the future.';
      return;
    }
    this.error = '';

    if (this.editMode) {
      const dto: UpdateAppointmentDto = {
        appointmentDate: new Date(this.form.appointmentDate).toISOString(),
        status: this.form.status
      };
      this.svc.update(this.form.id, dto).subscribe({
        next: () => { this.showModal = false; this.flash('Appointment updated successfully.'); this.load(); },
        error: err => { this.error = err?.error?.message || 'Update failed.'; }
      });
    } else {
      if (+this.form.patientId === 0 || +this.form.doctorId === 0) {
        this.error = 'Please select both a patient and a doctor.';
        return;
      }
      const dto: CreateAppointmentDto = {
        patientId: +this.form.patientId,
        doctorId: +this.form.doctorId,
        appointmentDate: new Date(this.form.appointmentDate).toISOString()
      };
      this.svc.create(dto).subscribe({
        next: () => { this.showModal = false; this.flash('Appointment scheduled successfully.'); this.load(); },
        error: err => { this.error = err?.error?.message || 'Scheduling failed.'; }
      });
    }
  }

  delete(id: number) {
    if (!confirm('Cancel this appointment?')) return;
    this.svc.delete(id).subscribe({
      next: () => { this.flash('Appointment cancelled.'); this.load(); },
      error: err => { this.error = err?.error?.message || 'Delete failed.'; }
    });
  }

  statusClass(s: string): string {
    if (s === 'Scheduled') return 'status-scheduled';
    if (s === 'Completed') return 'status-completed';
    if (s === 'Cancelled') return 'status-cancelled';
    return 'status-default';
  }

  private emptyForm() {
    return { id: 0, patientId: 0, doctorId: 0, appointmentDate: '', status: 'Scheduled' };
  }

  nowLocalIso(): string {
    const now = new Date();
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}` +
           `T${pad(now.getHours())}:${pad(now.getMinutes())}`;
  }

  private flash(msg: string) {
    this.success = msg;
    setTimeout(() => this.success = '', 3000);
  }
}
