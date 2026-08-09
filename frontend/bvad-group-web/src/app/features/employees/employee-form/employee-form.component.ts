import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import {
  CreateEmployeeRequest,
  Employee,
  ContractTypeValue,
  EmployeeStatusValue,
  GenderValue
} from '../../../core/models/employee.model';
import { EmployeeService } from '../../../core/services/employee.service';

@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './employee-form.component.html'
})
export class EmployeeFormComponent implements OnInit {
  @Input() employee: Employee | null = null;
  @Output() closed = new EventEmitter<boolean>();

  saving = signal(false);
  error = signal('');

  form: CreateEmployeeRequest = this.getEmpty();

  constructor(private empService: EmployeeService, public auth: AuthService) {}

  ngOnInit(): void {
    if (this.employee) {
      // Mode édition
      this.form = {
        firstName: this.employee.firstName,
        lastName: this.employee.lastName,
        middleName: this.employee.middleName,
        email: this.employee.email,
        phoneNumber: this.employee.phoneNumber,
        position: this.employee.position,
        department: this.employee.department,
        gender: GenderValue[this.employee.gender as keyof typeof GenderValue],
        birthDate: this.employee.birthDate?.substring(0, 10),
        hireDate: this.employee.hireDate.substring(0, 10),
        endDate: this.employee.endDate?.substring(0, 10),
        contractType: ContractTypeValue[this.employee.contractType as keyof typeof ContractTypeValue],
        salary: this.employee.salary,
        status: EmployeeStatusValue[this.employee.status as keyof typeof EmployeeStatusValue],
        city: this.employee.city,
        country: this.employee.country,
        companyId: this.employee.companyId,
        photoUrl: this.employee.photoUrl
      };
    } else {
      // Mode création : préremplir avec filiale active
      const current = this.auth.currentCompany();
     if (current && !current.isHolding) {
  this.form.companyId = current.id;
} else if (this.auth.companies().length > 0) {
  // Par défaut = première filiale non-holding
  const firstReal = this.auth.companies().find(c => !c.isHolding);
  if (firstReal) this.form.companyId = firstReal.id;
}
    }
  }

  submit(): void {
    if (!this.form.firstName || !this.form.lastName || !this.form.email || !this.form.position || !this.form.companyId) {
      this.error.set('Veuillez remplir tous les champs obligatoires');
      return;
    }

    this.saving.set(true);
    this.error.set('');

    // Convertir dates en ISO string
    const payload: any = { ...this.form };
    if (payload.birthDate) payload.birthDate = new Date(payload.birthDate).toISOString();
    if (payload.hireDate) payload.hireDate = new Date(payload.hireDate).toISOString();
    if (payload.endDate) payload.endDate = new Date(payload.endDate).toISOString();

    const request$ = this.employee
      ? this.empService.update(this.employee.id, payload)
      : this.empService.create(payload);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.closed.emit(true);
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err.error?.message || 'Erreur lors de l\'enregistrement');
        console.error(err);
      }
    });
  }

  cancel(): void {
    this.closed.emit(false);
  }

  private getEmpty(): CreateEmployeeRequest {
    return {
      firstName: '',
      lastName: '',
      email: '',
      position: '',
      gender: GenderValue.Male,
      hireDate: new Date().toISOString().substring(0, 10),
      contractType: ContractTypeValue.CDI,
      status: EmployeeStatusValue.Active,
      companyId: ''
    };
  }
}