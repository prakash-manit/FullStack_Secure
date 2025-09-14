import { Component, inject } from '@angular/core';
import { RoleFormComponent } from '../../components/role-form/role-form.component';
import { RoleService } from '../../services/role.service';
import { RoleCreateRequest } from '../../interfaces/role-create-request';
import {MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';
import { RoleListComponent } from '../../components/role-list/role-list.component';
import { AsyncPipe } from '@angular/common';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-role',
  standalone: true,
  imports: [RoleFormComponent, MatSnackBarModule, RoleListComponent, AsyncPipe, MatSelectModule, MatInputModule],
  templateUrl: './role.component.html',
  styleUrl: './role.component.scss'
})
export class RoleComponent {
  roleService = inject(RoleService);
  userService = inject(AuthService);
  errorMessage = '';
  role:RoleCreateRequest = {} as RoleCreateRequest;
  snackBar = inject(MatSnackBar);
  roles$ = this.roleService.getRoles();
  users$ = this.userService.getAll();
  selectedUserId:string = '';
  selectedRoleId:string = '';

  createRole(role:RoleCreateRequest){
    this.roleService.createRole(role).subscribe({
      next:(response: {message:string}) =>{
        this.roles$ = this.roleService.getRoles();
        this.snackBar.open("Role created successfully", "Close", { 
          duration: 3000
        });
      },
      error:(error:HttpErrorResponse) =>{
        if (error.status == 400) {
          this.errorMessage = error.error;
      }}
    });
  }

  deleteRole(id:string){
    this.roleService.deleteRole(id).subscribe({
      next:(response) =>{
        this.roles$ = this.roleService.getRoles();
        this.snackBar.open("Role deleted successfully", "Close", { 
          duration: 3000
        });
      },
      error:(error:HttpErrorResponse) =>{
        if (error.status == 400) {
          this.errorMessage = error.error;
      }}
    });  
  }

  assignRoleToUser(){
    this.roleService.assignRoleToUser(this.selectedUserId, this.selectedRoleId).subscribe({
      next:(response) =>{
        this.roles$ = this.roleService.getRoles();
        this.snackBar.open("Role assigned successfully", "Close", { 
          duration: 3000
        });
      },
      error:(error:HttpErrorResponse) =>{
        if (error.status == 400) {
          this.errorMessage = error.error;
      }}
    });  
  }
}
