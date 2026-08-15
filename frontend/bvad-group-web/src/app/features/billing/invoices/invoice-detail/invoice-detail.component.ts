import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { InvoiceService } from '../../../../core/services/invoice.service';
import { PaymentService } from '../../../../core/services/payment.service';
import {
  Invoice, InvoiceStatusColors, InvoiceStatusLabels, Payment,
  PaymentMethodLabels, formatMoney
} from '../../../../core/models/billing.model';
import { PaymentFormComponent } from '../payment-form/payment-form.component';

@Component({
  selector: 'app-invoice-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, PaymentFormComponent],
  templateUrl: './invoice-detail.component.html'
})
export class InvoiceDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private invoiceService = inject(InvoiceService);
  private paymentService = inject(PaymentService);

  invoice = signal<Invoice | null>(null);
  loading = signal(false);
  showPaymentForm = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.load(id);
  }

  load(id: string): void {
    this.loading.set(true);
    this.invoiceService.getById(id).subscribe({
      next: (data) => {
        this.invoice.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/invoices']);
      }
    });
  }

  reload(): void {
    const inv = this.invoice();
    if (inv) this.load(inv.id);
  }

  onPaymentClosed(reload: boolean): void {
    this.showPaymentForm.set(false);
    if (reload) this.reload();
  }

  downloadPdf(): void {
    const inv = this.invoice();
    if (!inv) return;
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

  downloadReceipt(p: Payment): void {
    this.paymentService.downloadReceipt(p.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Recu_${p.paymentNumber}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      }
    });
  }

  deletePayment(p: Payment): void {
    if (!confirm(`Supprimer ce paiement de ${this.money(p.amount)} ?`)) return;
    this.paymentService.delete(p.id).subscribe({
      next: () => this.reload()
    });
  }

  issue(): void {
    const inv = this.invoice();
    if (!inv) return;
    if (!confirm('Émettre cette facture ?')) return;
    this.invoiceService.issue(inv.id).subscribe({ next: () => this.reload() });
  }

  cancel(): void {
    const inv = this.invoice();
    if (!inv) return;
    if (!confirm('Annuler cette facture ?')) return;
    this.invoiceService.cancel(inv.id).subscribe({ next: () => this.reload() });
  }

  statusLabel(s: string): string {
    return InvoiceStatusLabels[s as keyof typeof InvoiceStatusLabels] || s;
  }

  statusColor(s: string): string {
    return InvoiceStatusColors[s as keyof typeof InvoiceStatusColors] || '';
  }

  paymentMethodLabel(m: string): string {
    return PaymentMethodLabels[m as keyof typeof PaymentMethodLabels] || m;
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('fr-FR', { day: '2-digit', month: 'long', year: 'numeric' });
  }

  money(amount: number, currency?: string): string {
    return formatMoney(amount, currency || this.invoice()?.currency || 'XAF');
  }
}