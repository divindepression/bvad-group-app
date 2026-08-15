import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ClientService } from '../../../../core/services/client.service';
import { Client } from '../../../../core/models/billing.model';
import { ClientFormComponent } from '../client-form/client-form.component';

@Component({
  selector: 'app-client-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ClientFormComponent],
  templateUrl: './client-list.component.html'
})
export class ClientListComponent implements OnInit {
  private clientService = inject(ClientService);

  clients = signal<Client[]>([]);
  loading = signal(false);
  search = '';
  showActiveOnly = signal(true);

  showForm = signal(false);
  editingClient = signal<Client | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.clientService.getAll(
      this.search || undefined,
      this.showActiveOnly() ? true : undefined
    ).subscribe({
      next: (data) => {
        this.clients.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onSearchChange(): void {
    this.load();
  }

  openAdd(): void {
    this.editingClient.set(null);
    this.showForm.set(true);
  }

  openEdit(c: Client): void {
    this.editingClient.set(c);
    this.showForm.set(true);
  }

  onFormClosed(reload: boolean): void {
    this.showForm.set(false);
    this.editingClient.set(null);
    if (reload) this.load();
  }

  delete(c: Client): void {
    if (!confirm(`Supprimer/désactiver le client "${c.displayName}" ?`)) return;

    this.clientService.delete(c.id).subscribe({
      next: () => this.load()
    });
  }

  // Stats
  totalCount = computed(() => this.clients().length);
  companyCount = computed(() => this.clients().filter(c => c.type === 'Company').length);
  individualCount = computed(() => this.clients().filter(c => c.type === 'Individual').length);

  initials(name: string): string {
    const parts = name.trim().split(' ');
    if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    return name.substring(0, 2).toUpperCase();
  }

  toggleActive(): void {
    this.showActiveOnly.update(v => !v);
    this.load();
  }
}