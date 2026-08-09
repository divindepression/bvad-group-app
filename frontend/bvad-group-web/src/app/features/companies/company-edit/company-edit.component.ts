import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CompanyService } from '../../../core/services/company.service';
import { Company, UpdateCompanyRequest } from '../../../core/models/company.model';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-company-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './company-edit.component.html'
})
export class CompanyEditComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private service = inject(CompanyService);
  auth = inject(AuthService);

  company = signal<Company | null>(null);
  loading = signal(false);
  saving = signal(false);
  message = signal('');
  deleting = signal(false);

  form: UpdateCompanyRequest = this.emptyForm();

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/companies']);
      return;
    }
    this.load(id);
  }

  load(id: string): void {
    this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (c) => {
        this.company.set(c);
        this.form = {
          name: c.name,
          legalName: c.legalName,
          description: c.description,
          slogan: c.slogan,
          color: c.color,
          registrationNumber: c.registrationNumber,
          taxNumber: c.taxNumber,
          address: c.address,
          city: c.city,
          country: c.country,
          phone: c.phone,
          email: c.email,
          website: c.website,
          directorName: c.directorName,
          directorTitle: c.directorTitle
        };
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  save(): void {
    const c = this.company();
    if (!c) return;

    this.saving.set(true);
    this.message.set('');

    this.service.update(c.id, this.form).subscribe({
      next: (updated) => {
        this.company.set(updated);
        this.saving.set(false);
        this.message.set('✅ Modifications enregistrées');
        setTimeout(() => this.message.set(''), 3000);
      },
      error: () => {
        this.saving.set(false);
        this.message.set('❌ Erreur lors de la sauvegarde');
      }
    });
  }

  // ═══════════════════════════════════════
  // 🖼 UPLOADS
  // ═══════════════════════════════════════
  onLogoSelected(event: Event): void {
    const file = this.getFile(event);
    if (!file) return;
    this.upload(file, 'logo');
  }

  onStampSelected(event: Event): void {
    const file = this.getFile(event);
    if (!file) return;
    this.upload(file, 'stamp');
  }

  onSignatureSelected(event: Event): void {
    const file = this.getFile(event);
    if (!file) return;
    this.upload(file, 'signature');
  }

  private getFile(event: Event): File | null {
    const input = event.target as HTMLInputElement;
    return input.files && input.files.length > 0 ? input.files[0] : null;
  }

  private upload(file: File, type: 'logo' | 'stamp' | 'signature'): void {
    const c = this.company();
    if (!c) return;

    const request$ = type === 'logo'
      ? this.service.uploadLogo(c.id, file)
      : type === 'stamp'
        ? this.service.uploadStamp(c.id, file)
        : this.service.uploadDirectorSignature(c.id, file);

    request$.subscribe({
      next: () => {
        this.service.refreshCache(c.id);
        this.load(c.id);
        this.message.set('✅ Fichier uploadé');
        setTimeout(() => this.message.set(''), 3000);
      },
      error: (err) => {
        this.message.set('❌ ' + (err.error?.message || 'Erreur upload'));
      }
    });
  }

  // ═══════════════════════════════════════
  // Helpers
  // ═══════════════════════════════════════
  logoUrl(): string {
    return this.service.logoUrl(this.company()!.id);
  }
  stampUrl(): string {
    return this.service.stampUrl(this.company()!.id);
  }
  signatureUrl(): string {
    return this.service.directorSignatureUrl(this.company()!.id);
  }

  private emptyForm(): UpdateCompanyRequest {
    return { name: '', color: '#1e3a8a' };
  }

  canDelete(): boolean {
  const role = this.auth?.currentUser()?.role;
  return role === 'SuperAdmin' && !this.company()?.isHolding;
}

deleteCompany(): void {
  const c = this.company();
  if (!c) return;

  if (!confirm(`⚠ Supprimer définitivement la filiale "${c.name}" ?\n\nCette action est irréversible. Elle ne fonctionnera que si aucun employé ni contrat n'y est rattaché.`)) {
    return;
  }

  this.deleting.set(true);
  this.service.delete(c.id).subscribe({
    next: () => {
      this.deleting.set(false);
      this.router.navigate(['/companies']);
    },
    error: (err) => {
      this.deleting.set(false);
      alert('❌ ' + (err.error?.message || 'Erreur suppression'));
    }
  });
}

}