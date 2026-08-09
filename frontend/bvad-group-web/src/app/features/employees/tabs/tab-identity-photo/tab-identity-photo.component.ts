import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { Employee } from '../../../../core/models/employee.model';
import { EmployeeDocumentService } from '../../../../core/services/employee-document.service';

@Component({
  selector: 'app-tab-identity-photo',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tab-identity-photo.component.html'
})
export class TabIdentityPhotoComponent {
  @Input({ required: true }) employee!: Employee;
  @Output() reload = new EventEmitter<void>();

  private docService = inject(EmployeeDocumentService);
  uploading = signal(false);
  previewUrl = signal<string | null>(null);

  get photoUrl(): string {
    return this.docService.identityPhotoUrl(this.employee.id);
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
    this.docService.uploadIdentityPhoto(this.employee.id, file).subscribe({
      next: () => {
        this.uploading.set(false);
        this.docService.refreshPhotoCache(this.employee.id);
        this.reload.emit();
      },
      error: (err) => {
        this.uploading.set(false);
        alert('❌ ' + (err.error?.message || 'Erreur upload'));
      }
    });
  }
}