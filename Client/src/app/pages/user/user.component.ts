import { Component, inject } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-user',
  standalone: true,
  imports: [AsyncPipe],
  templateUrl: './user.component.html',
  styleUrl: './user.component.scss'
})
export class UserComponent {
  authService=inject(AuthService);
  user$=this.authService.getAll();
}
