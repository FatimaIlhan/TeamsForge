import { Component } from '@angular/core';
import { Authservice } from '../../core/services/authservice';
import { UserDto } from '../../core/models';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardComponent {
  isLoading = false;
  errorMessage = '';
  user: UserDto | null = null;

  constructor(private authService: Authservice) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.getProfile().subscribe({
      next: (profile) => {
        this.user = profile;
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'Failed to load profile.';
      }
    });
  }

}
