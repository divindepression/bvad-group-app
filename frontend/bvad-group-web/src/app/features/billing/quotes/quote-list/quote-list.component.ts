import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { QuoteService } from '../../../../core/services/quote.service';
import {
  Quote,
  QuoteStatus,
  QuoteStatusColors,
  QuoteStatusLabels,
  formatMoney
} from '../../../../core/models/billing.model';
import { QuoteFormComponent } from '../quote-form/quote-form.component';

@Component({
  selector: 'app-quote-list',
  standalone: true,
  imports: [CommonModule, FormsModule, QuoteFormComponent],
  templateUrl: './quote-list.component.html'
})
export class QuoteListComponent {
  auth = inject(AuthService);
  private quoteService = inject(QuoteService);
  private router = inject(Router);

  quotes = signal<Quote[]>([]);
  loading = signal(false);
  statusFilter = signal<QuoteStatus | ''>('');

  showForm = signal(false);
  editingQuote = signal<Quote | null>(null);

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
    const status = this.statusFilter() || undefined;

    this.quoteService.getAll(companyId, undefined, status).subscribe({
      next: (data) => {
        this.quotes.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openAdd(): void {
    this.editingQuote.set(null);
    this.showForm.set(true);
  }

  openEdit(q: Quote): void {
    if (q.status === 'Converted' || q.status === 'Accepted') {
      alert('Ce devis ne peut plus être modifié.');
      return;
    }
    this.editingQuote.set(q);
    this.showForm.set(true);
  }

  onFormClosed(reload: boolean): void {
    this.showForm.set(false);
    this.editingQuote.set(null);
    if (reload) this.load();
  }

  markAsSent(q: Quote): void {
    this.quoteService.markAsSent(q.id).subscribe({ next: () => this.load() });
  }

  markAsAccepted(q: Quote): void {
    if (!confirm(`Marquer le devis ${q.quoteNumber} comme accepté ?`)) return;
    this.quoteService.markAsAccepted(q.id).subscribe({ next: () => this.load() });
  }

  markAsRejected(q: Quote): void {
    const reason = prompt('Motif du refus (optionnel) :');
    if (reason === null) return;
    this.quoteService.markAsRejected(q.id, reason).subscribe({ next: () => this.load() });
  }

  convertToInvoice(q: Quote): void {
    if (!confirm(`Convertir le devis ${q.quoteNumber} en facture ?`)) return;
    this.quoteService.convertToInvoice(q.id).subscribe({
      next: (invoice) => {
        alert(`✅ Facture ${invoice.invoiceNumber} créée !`);
        this.router.navigate(['/invoices']);
      },
      error: (err) => alert('❌ ' + (err.error?.message || 'Erreur'))
    });
  }

  downloadPdf(q: Quote): void {
    this.quoteService.downloadPdf(q.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Devis_${q.quoteNumber}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      }
    });
  }

  delete(q: Quote): void {
    if (!confirm(`Supprimer le devis ${q.quoteNumber} ?`)) return;
    this.quoteService.delete(q.id).subscribe({
      next: () => this.load(),
      error: (err) => alert('❌ ' + (err.error?.message || 'Erreur'))
    });
  }

  statusLabel(s: QuoteStatus): string {
    return QuoteStatusLabels[s] || s;
  }

  statusColor(s: QuoteStatus): string {
    return QuoteStatusColors[s] || '';
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  money(amount: number, currency: string): string {
    return formatMoney(amount, currency);
  }

  // Stats
  totalCount = computed(() => this.quotes().length);
  draftCount = computed(() => this.quotes().filter(q => q.status === 'Draft').length);
  sentCount = computed(() => this.quotes().filter(q => q.status === 'Sent').length);
  acceptedCount = computed(() => this.quotes().filter(q => q.status === 'Accepted' || q.status === 'Converted').length);
  totalAmount = computed(() => this.quotes().reduce((sum, q) => sum + q.totalTTC, 0));
}