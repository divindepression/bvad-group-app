import { CommonModule } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ContractService } from '../../../core/services/contract.service';
import {
  Contract,
  ContractStatus,
  ContractStatusColors,
  ContractStatusLabels,
  ContractTypeLabels
} from '../../../core/models/contract.model';
import { ContractFormComponent } from '../contract-form/contract-form.component';

@Component({
  selector: 'app-contract-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ContractFormComponent],
  templateUrl: './contract-list.component.html'
})
export class ContractListComponent {
  auth = inject(AuthService);
  private service = inject(ContractService);

  contracts = signal<Contract[]>([]);
  loading = signal(false);

  statusFilter = signal<ContractStatus | ''>('');
  expiringOnly = signal(false);

  showForm = signal(false);
  editingContract = signal<Contract | null>(null);

  constructor() {
    effect(() => {
      const c = this.auth.currentCompany();
      if (c) this.load();
    }, { allowSignalWrites: true });
  }

  load(): void {
    this.loading.set(true);
    const company = this.auth.currentCompany();
    const filters: any = {
      status: this.statusFilter() || undefined,
      expiringSoon: this.expiringOnly() || undefined
    };
    if (company && !company.isHolding) filters.companyId = company.id;

    this.service.getAll(filters).subscribe({
      next: (data) => {
        this.contracts.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openAdd(): void {
    this.editingContract.set(null);
    this.showForm.set(true);
  }

  openEdit(c: Contract): void {
    this.editingContract.set(c);
    this.showForm.set(true);
  }

  onFormClosed(reload: boolean): void {
    this.showForm.set(false);
    this.editingContract.set(null);
    if (reload) this.load();
  }

  delete(c: Contract): void {
    if (!confirm(`Supprimer le contrat ${c.contractNumber} ?`)) return;
    this.service.delete(c.id).subscribe({
      next: () => this.load()
    });
  }

  downloadPdf(c: Contract): void {
    this.service.downloadPdf(c.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Contrat_${c.contractNumber}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      }
    });
  }

  onFileSelected(event: Event, contract: Contract): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    this.service.uploadSigned(contract.id, file).subscribe({
      next: () => {
        alert('✅ Contrat signé uploadé');
        this.load();
      },
      error: (err) => alert('❌ ' + (err.error?.message || 'Erreur upload'))
    });
  }

  statusColor(s: ContractStatus): string {
    return ContractStatusColors[s] || 'bg-slate-500/20 text-slate-300';
  }
  statusLabel(s: ContractStatus): string {
    return ContractStatusLabels[s] || s;
  }
  typeLabel(t: any): string {
    return ContractTypeLabels[t as keyof typeof ContractTypeLabels] || t;
  }

  formatSalary(salary: number, currency: string): string {
    return new Intl.NumberFormat('fr-FR').format(salary) + ' ' + currency;
  }

  formatSize(bytes?: number): string {
    if (!bytes) return '';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  // Stats
  get totalCount(): number { return this.contracts().length; }
  get activeCount(): number { return this.contracts().filter(c => c.status === 'Active').length; }
  get expiringCount(): number { return this.contracts().filter(c => c.isExpiringSoon).length; }
}