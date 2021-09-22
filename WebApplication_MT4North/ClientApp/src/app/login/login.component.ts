import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';

import { AccountService, AlertService, ViewService } from '../_services';

@Component({ templateUrl: 'login.component.html' })

export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  loading = false;
  submitted = false;
  returnUrl: string;
  error = '';

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private accountService: AccountService,
    private alertService: AlertService,
    private viewservice: ViewService
  ) {
    // redirect to home if already logged in
    if (this.accountService.currentUserValue) {
      this.router.navigate(['/my-pages/start']);
    }
  }

  ngOnInit() {
    this.loginForm = this.formBuilder.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });

    // get return url from route parameters or default to '/'
    this.router.navigate(['/my-pages/start']);
    // always return to 'mina-sidor' when after loggin in.
    this.returnUrl = '/my-pages/start';
    //todo: fix this: this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/my-pages/start';
    // user needs to be redirected to 'mina-sidor', otherwise the data is not fetced from the the api correctly
  }

  // convenience getter for easy access to form fields
  get f() { return this.loginForm.controls; }

  onSubmit() {
    this.submitted = true;

    // reset alerts on submit
    this.alertService.clear();

    // stop here if form is invalid
    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;
    this.accountService.login(this.f.username.value, this.f.password.value)
      .pipe(first())
      .subscribe(
        data => {
          this.router.navigate([this.returnUrl]);
        },
        error => {
          const err = error.error.message || error.statusText;
          this.error = err;
          this.alertService.error(err);
          this.loading = false;
        });
  }
}

//import { sharedStylesheetJitUrl } from '@angular/compiler';
//import { Component } from '@angular/core';

//export class Student {
//  public name: string;
//  public email: string;
//  public password: string;
//  public subjects: string;
//}

//@Component({
//  selector: 'app-login',
//  templateUrl: './login.component.html',
//  styleUrls: ['./login.component.css'],
 
//})
//export class LoginComponent {
//  model = new Student();

//  submit(data) {
//    console.log(data.value)
//  }
//}
