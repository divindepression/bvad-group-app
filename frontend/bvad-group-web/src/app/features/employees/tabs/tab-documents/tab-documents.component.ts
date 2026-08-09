import { CommonModule } from '@angular/common';
import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Employee } from '../../../../core/models/employee.model';
import { EmployeeDocumentService } from '../../../../core/services/employee-document.service';
import {
  CreateDocumentMetadata,
  DocumentTypeIcons,
  DocumentTypeLabels,
  DocumentTypeValue,
  EmployeeDocument
} from '../../../../core/models/employee-document.model';

@Component({
  selector: 'app-tab-documents',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tab-documents.component.html'
})
export class TabDocumentsComponent implements OnInit {
  @Input({ required: true }) employee!: Employee;

  private service = inject(EmployeeDocumentService);
  documents = signal<EmployeeDocument[]>([]);
  loading = signal(false);
  uploading = signal(false);
  showForm = signal(false);

  metadata: CreateDocumentMetadata = {
    type: DocumentTypeValue.CV,
    title: ''
  };
  selectedFile: File | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getByEmployee(this.employee.id).subscribe({
      next: (data) => {
        this.documents.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openForm(): void {
    this.metadata = { type: DocumentTypeValue.CV, title: '' };
    this.selectedFile = null;
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.selectedFile = null;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      if (!this.metadata.title) {
        this.metadata.title = this.selectedFile.name.replace(/\.[^.]+$/, '');
      }
    }
  }

  upload(): void {
    if (!this.selectedFile || !this.metadata.title) {
      alert('⚠ Fichier et titre requis');
      return;
    }

    this.uploading.set(true);
    this.service.upload(this.employee.id, this.selectedFile, this.metadata).subscribe({
      next: () => {
        this.uploading.set(false);
        this.closeForm();
        this.load();
      },
      error: (err) => {
        this.uploading.set(false);
        alert('❌ ' + (err.error?.message || 'Erreur upload'));
      }
    });
  }

  download(doc: EmployeeDocument): void {
    const url = this.service.downloadUrl(this.employee.id, doc.id);
    window.open(url, '_blank');
  }

  delete(doc: EmployeeDocument): void {
    if (!confirm(`Supprimer "${doc.title}" ?`)) return;
    this.service.delete(this.employee.id, doc.id).subscribe({
      next: () => this.load()
    });
  }

  icon(type: string): string {
    return DocumentTypeIcons[type as keyof typeof DocumentTypeIcons] || '📎';
  }

  label(type: string): string {
    return DocumentTypeLabels[type as keyof typeof DocumentTypeLabels] || type;
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}