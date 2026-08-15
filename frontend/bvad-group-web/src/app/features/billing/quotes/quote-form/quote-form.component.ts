import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { ClientService } from '../../../../core/services/client.service';
import { CompanyService } from '../../../../core/services/company.service';
import { QuoteService } from '../../../../core/services/quote.service';
import { Client,  CreateQuoteRequest, LineItem, Quote, formatMoney } from '../../../../core/models/billing.model';
import { Company } from '../../../../core/models/company.model';

@Component({
  selector: 'app-quote-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './quote-form.component.html'
})
export class QuoteFormComponent implements OnInit {
  @Input() quote: Quote | null = null;
  @Output() closed = new EventEmitter<boolean>();

  private quoteService = inject(QuoteService);
  private clientService = inject(ClientService);
  private companyService = inject(CompanyService);
  auth = inject(AuthService);

  saving = signal(false);
  error = signal('');
  clients = signal<Client[]>([]);
  companies = signal<Company[]>([]);

  form: CreateQuoteRequest = this.getEmpty();

  ngOnInit(): void {
    this.loadClients();
    this.loadCompanies();

    if (this.quote) {
      this.form = {
        companyId: this.quote.companyId,
        clientId: this.quote.clientId,
        issueDate: this.quote.issueDate.substring(0, 10),
        validUntil: this.quote.validUntil.substring(0, 10),
        currency: this.quote.currency,
        vatRate: this.quote.vatRate,
        subject: this.quote.subject,
        notes: this.quote.notes,
        paymentTerms: this.quote.paymentTerms,
        discountPercent: this.quote.discountPercent,
        lineItems: this.quote.lineItems.map(l => ({
          order: l.order,
          description: l.description,
          unit: l.unit,
          quantity: l.quantity,
          unitPrice: l.unitPrice,
          discountPercent: l.discountPercent
        }))
      };
    } else {
      const current = this.auth.currentCompany();
      if (current && !current.isHolding) {
        this.form.companyId = current.id;
      }
    }
  }

  loadClients(): void {
    this.clientService.getAll(undefined, true).subscribe({
      next: (data) => this.clients.set(data)
    });
  }

  loadCompanies(): void {
    this.companyService.getAll().subscribe({
      next: (data) => this.companies.set(data.filter(c => !c.isHolding))
    });
  }

  // ═══════════════════════════════════════
  // Gestion des lignes dynamiques
  // ═══════════════════════════════════════
  addLine(): void {
    const order = this.form.lineItems.length + 1;
    this.form.lineItems.push({
      order,
      description: '',
      unit: '',
      quantity: 1,
      unitPrice: 0,
      discountPercent: 0
    });
  }

  removeLine(index: number): void {
    this.form.lineItems.splice(index, 1);
    // Réordonner
    this.form.lineItems.forEach((l, i) => l.order = i + 1);
  }

  moveUp(index: number): void {
    if (index === 0) return;
    const tmp = this.form.lineItems[index - 1];
    this.form.lineItems[index - 1] = this.form.lineItems[index];
    this.form.lineItems[index] = tmp;
    this.form.lineItems.forEach((l, i) => l.order = i + 1);
  }

  moveDown(index: number): void {
    if (index === this.form.lineItems.length - 1) return;
    const tmp = this.form.lineItems[index + 1];
    this.form.lineItems[index + 1] = this.form.lineItems[index];
    this.form.lineItems[index] = tmp;
    this.form.lineItems.forEach((l, i) => l.order = i + 1);
  }

  lineTotal(line: LineItem): number {
    return line.quantity * line.unitPrice * (1 - line.discountPercent / 100);
  }

  // ═══════════════════════════════════════
  // Totaux calculés en live
  // ═══════════════════════════════════════
  get subtotalHT(): number {
    return this.form.lineItems.reduce((sum, l) => sum + this.lineTotal(l), 0);
  }

  get discountAmount(): number {
    return this.subtotalHT * (this.form.discountPercent / 100);
  }

  get afterDiscount(): number {
    return this.subtotalHT - this.discountAmount;
  }

  get vatAmount(): number {
    return this.afterDiscount * (this.form.vatRate / 100);
  }

  get totalTTC(): number {
    return this.afterDiscount + this.vatAmount;
  }

  // ═══════════════════════════════════════
  submit(): void {
    if (!this.form.companyId || !this.form.clientId) {
      this.error.set('Sélectionnez une filiale et un client');
      return;
    }

    if (this.form.lineItems.length === 0) {
      this.error.set('Ajoutez au moins une ligne au devis');
      return;
    }

    // Vérifier lignes
    for (const line of this.form.lineItems) {
      if (!line.description?.trim() || line.quantity <= 0 || line.unitPrice < 0) {
        this.error.set('Toutes les lignes doivent avoir une description, quantité > 0 et prix ≥ 0');
        return;
      }
    }

    this.saving.set(true);
    this.error.set('');

    const payload: CreateQuoteRequest = {
      ...this.form,
      issueDate: new Date(this.form.issueDate).toISOString(),
      validUntil: new Date(this.form.validUntil).toISOString()
    };

    const request$ = this.quote
      ? this.quoteService.update(this.quote.id, payload)
      : this.quoteService.create(payload);

    request$.subscribe({
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
    return formatMoney(amount, this.form.currency);
  }

  private getEmpty(): CreateQuoteRequest {
    const today = new Date().toISOString().substring(0, 10);
    const in30days = new Date();
    in30days.setDate(in30days.getDate() + 30);

    return {
      companyId: '',
      clientId: '',
      issueDate: today,
      validUntil: in30days.toISOString().substring(0, 10),
      currency: 'XAF',
      vatRate: 18,
      subject: '',
      notes: '',
      paymentTerms: '30 jours à réception',
      discountPercent: 0,
      lineItems: [{
        order: 1,
        description: '',
        unit: '',
        quantity: 1,
        unitPrice: 0,
        discountPercent: 0
      }]
    };
  }
}