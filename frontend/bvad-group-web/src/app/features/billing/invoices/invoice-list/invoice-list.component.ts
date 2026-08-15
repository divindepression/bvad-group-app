import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { InvoiceService } from '../../../../core/services/invoice.service';
import {
  Invoice,
  InvoiceStatus,
  InvoiceStatusColors,
  InvoiceStatusLabels,
  formatMoney
} from '../../../../core/models/billing.model';
import { InvoiceFormComponent } from '../invoice-form/invoice-form.component';

@Component({
  selector: 'app-invoice-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, InvoiceFormComponent],
  templateUrl: './invoice-list.component.html'
})
export class InvoiceListComponent {
  auth = inject(AuthService);
  private invoiceService = inject(InvoiceService);

  invoices = signal<Invoice[]>([]);
  loading = signal(false);
  statusFilter = signal<InvoiceStatus | ''>('');
  overdueOnly = signal(false);

  showForm = signal(false);
  editingInvoice = signal<Invoice | null>(null);

  constructor() {
    effect(() => {
      const c = this.auth.currentCompany();
      if (c) this.load();
    }, { allowSignalWrites: true });
  }

  load(): void {
    this.loading.set(true);
    const company = this.auth.currentCompany();
    const companyId = company && !company.isHolding ? company.id : undefined;

    this.invoiceService.getAll(
      companyId,
      undefined,
      this.statusFilter() || undefined,
      this.overdueOnly() ? true : undefined
    ).subscribe({
      next: (data) => {
        this.invoices.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  toggleOverdue(): void {
  this.overdueOnly.set(!this.overdueOnly());
  this.load();
}

  openAdd(): void {
    this.editingInvoice.set(null);
    this.showForm.set(true);
  }

  openEdit(inv: Invoice): void {
    if (inv.status !== 'Draft') {
      alert('Une facture émise ne peut plus être modifiée.');
      return;
    }
    this.editingInvoice.set(inv);
    this.showForm.set(true);
  }

  onFormClosed(reload: boolean): void {
    this.showForm.set(false);
    this.editingInvoice.set(null);
    if (reload) this.load();
  }

  issue(inv: Invoice): void {
    if (!confirm(`Émettre la facture ${inv.invoiceNumber} ?\n\nUne fois émise, elle ne pourra plus être modifiée.`)) return;
    this.invoiceService.issue(inv.id).subscribe({
      next: () => this.load()
    });
  }

  cancel(inv: Invoice): void {
    if (!confirm(`Annuler la facture ${inv.invoiceNumber} ?`)) return;
    this.invoiceService.cancel(inv.id).subscribe({
      next: () => this.load()
    });
  }

  delete(inv: Invoice): void {
    if (!confirm(`Supprimer le brouillon ${inv.invoiceNumber} ?`)) return;
    this.invoiceService.delete(inv.id).subscribe({
      next: () => this.load(),
      error: (err) => alert('❌ ' + (err.error?.message || 'Erreur'))
    });
  }

  downloadPdf(inv: Invoice): void {
    this.invoiceService.downloadPdf(inv.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Facture_${inv.invoiceNumber}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      }
    });
  }

  statusLabel(s: InvoiceStatus): string {
    return InvoiceStatusLabels[s] || s;
  }

  statusColor(s: InvoiceStatus): string {
    return InvoiceStatusColors[s] || '';
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  money(amount: number, currency: string): string {
    return formatMoney(amount, currency);
  }

  // Stats
  totalCount = computed(() => this.invoices().length);
  paidCount = computed(() => this.invoices().filter(i => i.status === 'Paid').length);
  unpaidCount = computed(() =>
    this.invoices().filter(i =>
      i.status === 'Issued' || i.status === 'PartiallyPaid' || i.status === 'Overdue'
    ).length
  );
  overdueCount = computed(() => this.invoices().filter(i => i.isOverdue).length);
  totalAmount = computed(() => this.invoices().reduce((sum, i) => sum + i.totalTTC, 0));
  totalPaid = computed(() => this.invoices().reduce((sum, i) => sum + i.amountPaid, 0));
  totalDue = computed(() => this.invoices().reduce((sum, i) => sum + i.amountDue, 0));

  displayCurrency(): string {
    const first = this.invoices()[0];
    return first?.currency || 'XAF';
  }
}