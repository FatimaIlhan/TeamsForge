import { Component, OnInit } from '@angular/core';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Authservice } from '../../../core/services/authservice';

@Component({
  selector: 'app-resetpassword',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, CommonModule],
  templateUrl: './resetpassword.html',
  styleUrl: './resetpassword.css',
})
export class ResetpasswordComponent implements OnInit {
  resetPasswordForm: FormGroup;
  isLoading = false;
  successMessage = '';
  errorMessage = '';
  email = '';
  token = '';

  constructor(
    private fb: FormBuilder,
    private authService: Authservice,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.resetPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      token: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    // Get email and token from query params if available
    this.route.queryParams.subscribe(params => {
      if (params['email']) {
        this.email = params['email'];
        this.resetPasswordForm.patchValue({ email: this.email });
      }
      if (params['token']) {
        this.token = params['token'];
        this.resetPasswordForm.patchValue({ token: this.token });
      }
    });
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('newPassword')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    
    if (password !== confirmPassword) {
      return { passwordMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.resetPasswordForm.invalid) {
      this.resetPasswordForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const { email, token, newPassword } = this.resetPasswordForm.value;

    this.authService.resetPassword({ email, token, newPassword }).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.successMessage = response.message;
        
        // Navigate to login after successful reset
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'Failed to reset password. Please try again.';
      }
    });
  }

  get emailControl() {
    return this.resetPasswordForm.get('email');
  }

  get tokenControl() {
    return this.resetPasswordForm.get('token');
  }

  get newPassword() {
    return this.resetPasswordForm.get('newPassword');
  }

  get confirmPassword() {
    return this.resetPasswordForm.get('confirmPassword');
  }
}
