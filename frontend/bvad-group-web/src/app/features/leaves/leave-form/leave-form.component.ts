import { CommonModule } from '@angular/common';
import { Component, EventEmitter, OnInit, Output, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MeService } from '../../../core/services/me.service';
import { LeaveService } from '../../../core/services/leave.service';
import {
  CreateLeaveRequest,
  LeaveBalance,
  LeaveType
} from '../../../core/models/leave.model';

@Component({
  selector: 'app-leave-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './leave-form.component.html'
})
export class LeaveFormComponent implements OnInit {
  @Output() closed = new EventEmitter<boolean>();

  private leaveService = inject(LeaveService);
  meService = inject(MeService);

  types = signal<LeaveType[]>([]);
  balances = signal<LeaveBalance[]>([]);
  saving = signal(false);
  error = signal('');
  loading = signal(true);

  // Dates par défaut : aujourd'hui → dans 5 jours
  private today = new Date();
  private fiveDaysLater = new Date();

  form: CreateLeaveRequest = {
    employeeId: '',
    leaveTypeId: '',
    startDate: '',
    endDate: '',
    isHalfDay: false,
    reason: ''
  };

  constructor() {
    // Init dates
    this.fiveDaysLater.setDate(this.fiveDaysLater.getDate() + 5);
    this.form.startDate = this.formatDateForInput(this.today);
    this.form.endDate = this.formatDateForInput(this.fiveDaysLater);

    // 🔥 Réagit dès que le profil est chargé
    effect(() => {
      const profile = this.meService.profile();
      if (profile?.employee) {
        this.form.employeeId = profile.employee.id;
        this.loadBalances(profile.employee.id);
      }
    }, { allowSignalWrites: true });
  }

  ngOnInit(): void {
    // Charger les types
    this.leaveService.getTypes().subscribe({
      next: (t) => this.types.set(t)
    });

    // Charger le profil (au cas où pas encore chargé)
    this.meService.load();
  }

  private loadBalances(employeeId: string): void {
    this.leaveService.getBalances(employeeId).subscribe({
      next: (b) => {
        this.balances.set(b);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  private formatDateForInput(d: Date): string {
    return d.toISOString().substring(0, 10);
  }

  // ═══ Calcul jours ouvrés (aperçu) ═══
  get calculatedDays(): number {
    if (!this.form.startDate || !this.form.endDate) return 0;

    const start = new Date(this.form.startDate);
    const end = new Date(this.form.endDate);
    if (end < start) return 0;

    let count = 0;
    const current = new Date(start);
    while (current <= end) {
      const day = current.getDay();
      if (day !== 0 && day !== 6) count++;
      current.setDate(current.getDate() + 1);
    }

    if (this.form.isHalfDay && count === 1) return 0.5;
    return count;
  }

  get selectedType(): LeaveType | undefined {
    return this.types().find(t => t.id === this.form.leaveTypeId);
  }

  get selectedBalance(): LeaveBalance | undefined {
    return this.balances().find(b => b.leaveTypeId === this.form.leaveTypeId);
  }

  get insufficientBalance(): boolean {
    const type = this.selectedType;
    const balance = this.selectedBalance;
    if (!type?.decrementsBalance || !balance) return false;
    return this.calculatedDays > balance.remainingDays;
  }

  submit(): void {
    if (!this.form.employeeId) {
      this.error.set('Chargement du profil en cours, veuillez patienter...');
      this.meService.load();
      return;
    }

    if (!this.form.leaveTypeId || !this.form.startDate || !this.form.endDate) {
      this.error.set('Veuillez remplir tous les champs obligatoires');
      return;
    }

    if (new Date(this.form.endDate) < new Date(this.form.startDate)) {
      this.error.set('La date de fin doit être postérieure à la date de début');
      return;
    }

    if (this.calculatedDays === 0) {
      this.error.set('Aucun jour ouvré dans la période sélectionnée');
      return;
    }

    this.saving.set(true);
    this.error.set('');

    const payload = {
      ...this.form,
      startDate: new Date(this.form.startDate).toISOString(),
      endDate: new Date(this.form.endDate).toISOString()
    };

    this.leaveService.create(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.closed.emit(true);
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err.error?.message || 'Erreur lors de la création');
      }
    });
  }

  cancel(): void {
    this.closed.emit(false);
  }
}