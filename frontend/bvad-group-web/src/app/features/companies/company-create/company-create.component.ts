import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CompanyService } from '../../../core/services/company.service';
import { CreateCompanyRequest } from '../../../core/models/company.model';

@Component({
  selector: 'app-company-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './company-create.component.html'
})
export class CompanyCreateComponent {
  @Output() closed = new EventEmitter<boolean>();

  private service = inject(CompanyService);
  saving = signal(false);
  error = signal('');

  // Suggestions d'emoji
  emojiChoices = ['🏢', '🌾', '💻', '🎓', '💼', '🏥', '🚛', '🏗', '🍔', '⚡', '📚', '🎨', '🏨', '🛒', '⚙️'];

  // Suggestions couleurs pré-définies
  colorChoices = [
    { name: 'Bleu', value: '#1e3a8a' },
    { name: 'Vert', value: '#16a34a' },
    { name: 'Cyan', value: '#0891b2' },
    { name: 'Orange', value: '#ea580c' },
    { name: 'Violet', value: '#7c3aed' },
    { name: 'Rouge', value: '#dc2626' },
    { name: 'Rose', value: '#ec4899' },
    { name: 'Ambre', value: '#f59e0b' },
    { name: 'Émeraude', value: '#10b981' },
    { name: 'Indigo', value: '#6366f1' }
  ];

  form: CreateCompanyRequest = this.emptyForm();

  submit(): void {
    if (!this.form.code || !this.form.name) {
      this.error.set('Code et nom obligatoires');
      return;
    }

    this.saving.set(true);
    this.error.set('');

    this.service.create(this.form).subscribe({
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

  pickEmoji(emoji: string): void {
    this.form.logo = emoji;
  }

  pickColor(color: string): void {
    this.form.color = color;
  }

  private emptyForm(): CreateCompanyRequest {
    return {
      code: '',
      name: '',
      color: '#1e3a8a',
      logo: '🏢',
      isHolding: false,
      displayOrder: 10,
      country: 'Cameroun'
    };
  }
}