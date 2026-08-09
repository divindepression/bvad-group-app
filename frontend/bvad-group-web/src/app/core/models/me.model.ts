import { Employee } from './employee.model';

export interface MyProfile {
  userId: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  role: string;
  photoUrl?: string;
  employee?: Employee;
}