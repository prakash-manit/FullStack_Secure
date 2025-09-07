import { Component, inject } from '@angular/core';
import { RoleFormComponent } from '../../components/role-form/role-form.component';
import { RoleService } from '../../services/role.service';
import { RoleCreateRequest } from '../../interfaces/role-create-request';

@Component({
  selector: 'app-role',
  standalone: true,
  imports: [RoleFormComponent],
  templateUrl: './role.component.html',
  styleUrl: './role.component.scss'
})
export class RoleComponent {
  roleService = inject(RoleService);
  errorMessage = '';
  role:RoleCreateRequest = {} as RoleCreateRequest;

  createRole(role:RoleCreateRequest){
  //   this.roleService.createRole(role).subscribe({
  //     next: (response) => {
  //       console.log('Role created successfully:', response);
  //       this.errorMessage = '';
  //     },
  //     error: (error) => {
  //       console.error('Error creating role:', error);
  //       this.errorMessage = 'Failed to create role. Please try again.';
  //     }
  //   });
  }
}
