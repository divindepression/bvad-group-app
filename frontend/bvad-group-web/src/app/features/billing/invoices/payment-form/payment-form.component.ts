import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../../../core/services/payment.service';
import {
  CreatePaymentRequest, Invoice, MobileMoneyOperatorValue, PaymentMethodValue, formatMoney
} from '../../../../core/models/billing.model';

@Component({
  selector: 'app-payment-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payment-form.component.html'
})
export class PaymentFormComponent {
  @Input({ required: true }) invoice!: Invoice;
  @Output() closed = new EventEmitter<boolean>();

  private paymentService = inject(PaymentService);
  saving = signal(false);
  error = signal('');

  form: CreatePaymentRequest = {
    invoiceId: '',
    amount: 0,
    currency: 'XAF',
    paymentDate: new Date().toISOString().substring(0, 10),
    method: PaymentMethodValue.Cash
  };

  ngOnInit(): void {
    this.form.invoiceId = this.invoice.id;
    this.form.currency = this.invoice.currency;
    this.form.amount = this.invoice.amountDue;
  }

  get isMobileMoney(): boolean {
    return this.form.method === PaymentMethodValue.MobileMoney;
  }

  setFullAmount(): void {
    this.form.amount = this.invoice.amountDue;
  }

  submit(): void {
    if (this.form.amount <= 0) {
      this.error.set('Le montant doit être positif');
      return;
    }

    if (this.form.amount > this.invoice.amountDue) {
      this.error.set(`Le montant dépasse le solde dû (${this.money(this.invoice.amountDue)})`);
      return;
    }

    this.saving.set(true);
    this.error.set('');

    const payload: CreatePaymentRequest = {
      ...this.form,
      paymentDate: new Date(this.form.paymentDate).toISOString()
    };

    this.paymentService.record(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.closed.emit(true);
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err.error?.message || 'Erreur');
      }
    });
  }

  cancel(): void {
    this.closed.emit(false);
  }

  money(amount: number): string {
    return formatMoney(amount, this.invoice.currency);
  }
}