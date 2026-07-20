import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PatientService } from '../../services/patient';
import { DoctorService } from '../../services/doctor';
import { AppointmentService } from '../../services/appointment';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  patientCount = 0;
  doctorCount = 0;
  appointmentCount = 0;
  scheduledCount = 0;

  constructor(
    private patientSvc: PatientService,
    private doctorSvc: DoctorService,
    private apptSvc: AppointmentService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.patientSvc.getAll().subscribe(p => {
      this.patientCount = p.length;
      this.cdr.markForCheck();
    });
    this.doctorSvc.getAll().subscribe(d => {
      this.doctorCount = d.length;
      this.cdr.markForCheck();
    });
    this.apptSvc.getAll().subscribe(a => {
      this.appointmentCount = a.length;
      this.scheduledCount = a.filter(x => x.status === 'Scheduled').length;
      this.cdr.markForCheck();
    });
  }
}
