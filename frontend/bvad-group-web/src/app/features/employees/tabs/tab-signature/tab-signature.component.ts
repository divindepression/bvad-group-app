import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Employee } from '../../../../core/models/employee.model';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-tab-signature',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tab-signature.component.html'
})
export class TabSignatureComponent {
  @Input({ required: true }) employee!: Employee;
  @Output() reload = new EventEmitter<void>();

  private http = inject(HttpClient);
  uploading = signal(false);
  previewUrl = signal<string | null>(null);

  get signatureUrl(): string {
    return `${environment.apiUrl}/employees/${this.employee.id}/signature?t=${Date.now()}`;
  }

  onFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];

    // Preview local
    const reader = new FileReader();
    reader.onload = (e) => this.previewUrl.set(e.target?.result as string);
    reader.readAsDataURL(file);

    // Upload
    this.uploading.set(true);
    const formData = new FormData();
    formData.append('file', file);

    this.http.post(`${environment.apiUrl}/employees/${this.employee.id}/signature`, formData).subscribe({
      next: () => {
        this.uploading.set(false);
        this.reload.emit();
      },
      error: (err) => {
        this.uploading.set(false);
        alert('❌ ' + (err.error?.message || 'Erreur'));
      }
    });
  }
}