import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ClientService } from '../../../../core/services/client.service';
import {
  Client,
  ClientTypeValue,
  CreateClientRequest
} from '../../../../core/models/billing.model';

@Component({
  selector: 'app-client-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './client-form.component.html'
})
export class ClientFormComponent implements OnInit {
  @Input() client: Client | null = null;
  @Output() closed = new EventEmitter<boolean>();

  private service = inject(ClientService);
  saving = signal(false);
  error = signal('');

  form: CreateClientRequest = {
    type: ClientTypeValue.Company,
    name: '',
    country: 'Congo'
  };

  ngOnInit(): void {
    if (this.client) {
      this.form = {
        type: this.client.type === 'Company' ? ClientTypeValue.Company : ClientTypeValue.Individual,
        name: this.client.name,
        contactPerson: this.client.contactPerson,
        position: this.client.position,
        legalForm: this.client.legalForm,
        registrationNumber: this.client.registrationNumber,
        taxNumber: this.client.taxNumber,
        capital: this.client.capital,
        email: this.client.email,
        phone: this.client.phone,
        secondaryPhone: this.client.secondaryPhone,
        website: this.client.website,
        address: this.client.address,
        city: this.client.city,
        country: this.client.country,
        postalCode: this.client.postalCode,
        notes: this.client.notes
      };
    }
  }

  get isCompany(): boolean {
    return this.form.type === ClientTypeValue.Company;
  }

  submit(): void {
    if (!this.form.name?.trim()) {
      this.error.set('Le nom est obligatoire');
      return;
    }

    this.saving.set(true);
    this.error.set('');

    const request$ = this.client
      ? this.service.update(this.client.id, this.form)
      : this.service.create(this.form);

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
}