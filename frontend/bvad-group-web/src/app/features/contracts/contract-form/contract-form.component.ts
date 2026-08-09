import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ContractService } from '../../../core/services/contract.service';
import { EmployeeService } from '../../../core/services/employee.service';
import { Employee } from '../../../core/models/employee.model';
import {
  Contract,
  ContractStatusValue,
  ContractTypeValueMap,
  CreateContractRequest
} from '../../../core/models/contract.model';

@Component({
  selector: 'app-contract-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contract-form.component.html'
})
export class ContractFormComponent implements OnInit {
  @Input() contract: Contract | null = null;
  @Output() closed = new EventEmitter<boolean>();

  private service = inject(ContractService);
  private empService = inject(EmployeeService);
  auth = inject(AuthService);

  saving = signal(false);
  error = signal('');
  employees = signal<Employee[]>([]);

  form: CreateContractRequest = this.getEmpty();

  ngOnInit(): void {
    // Charger les employés de la filiale active
    const company = this.auth.currentCompany();
    const filters: any = {};
    if (company && !company.isHolding) filters.companyId = company.id;

    this.empService.getAll(filters).subscribe({
      next: (data) => this.employees.set(data)
    });

    if (this.contract) {
      this.form = {
        employeeId: this.contract.employeeId,
        contractType: ContractTypeValueMap[this.contract.contractType],
        position: this.contract.position,
        department: this.contract.department,
        startDate: this.contract.startDate.substring(0, 10),
        endDate: this.contract.endDate?.substring(0, 10),
        signedDate: this.contract.signedDate?.substring(0, 10),
        salary: this.contract.salary,
        currency: this.contract.currency,
        trialPeriodMonths: this.contract.trialPeriodMonths,
        weeklyHours: this.contract.weeklyHours,
        specialClauses: this.contract.specialClauses,
        notes: this.contract.notes,
        status: ContractStatusValue[this.contract.status]
      };
    }
  }

  submit(): void {
    if (!this.form.employeeId || !this.form.position || !this.form.salary) {
      this.error.set('Champs obligatoires manquants');
      return;
    }

    this.saving.set(true);
    this.error.set('');

    const payload: any = { ...this.form };
    payload.startDate = new Date(payload.startDate).toISOString();
    if (payload.endDate) payload.endDate = new Date(payload.endDate).toISOString();
    if (payload.signedDate) payload.signedDate = new Date(payload.signedDate).toISOString();

    const request$ = this.contract
      ? this.service.update(this.contract.id, payload)
      : this.service.create(payload);

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

  private getEmpty(): CreateContractRequest {
    return {
      employeeId: '',
      contractType: ContractTypeValueMap.CDI,
      position: '',
      startDate: new Date().toISOString().substring(0, 10),
      salary: 0,
      currency: 'FCFA',
      weeklyHours: 40,
      status: ContractStatusValue.Draft
    };
  }
}