import { Component, OnDestroy, OnInit, ElementRef, ViewChild } from '@angular/core';
import { ViewService } from "../_services";
import { AccountService, AlertService } from '@app/_services';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { first } from 'rxjs/operators';
import { User, Alert, AlertType } from '../_models';
import { Subscription } from 'rxjs';


@Component({
  selector: 'app-my-pages-edit-account',
  templateUrl: './my-pages-edit-account.component.html',
  styleUrls: ['./my-pages-edit-account.component.css']
})

export class MyPagesEditAccountComponent implements OnDestroy {
  form: FormGroup;
  loading = false;
  submitted = false;
  currentUser: User;
  accountSubscription: Subscription;
  public gender_options: any[] = [{ id: null, name: "Vill inte ange" }, { id: 0, name: "Kvinna" }, { id: 1, name: "Man" }, { id: 2, name: "Annat" }];

  constructor(
    private viewService: ViewService,
    private accountService: AccountService,
    private alertService: AlertService,
    private formBuilder: FormBuilder,

  ) {
    this.accountSubscription = this.accountService.currentUser.subscribe(x => this.currentUser = x);
  }

  ngOnInit() {
    this.form = this.formBuilder.group({
      firstname: [this.currentUser.firstname, Validators.required],
      lastname: [this.currentUser.lastname, Validators.required],
      email: [this.currentUser.email, [Validators.required, Validators.email]],
      gender: [this.gender_options.find(x => x.id == this.currentUser.gender)],
      company: [this.currentUser.companyname],
      country: [this.currentUser.country]
    });
  }

  ngOnDestroy() {
    this.accountSubscription.unsubscribe();
  }

  isFullscreen() {
    return this.viewService.isFullscreen;
  }


  // convenience getter for easy access to form fields
  get f() { return this.form.controls; }

  onSubmit() {
    this.submitted = true;

    // reset alerts on submit
    this.alertService.clear();

    // stop here if form is invalid
    if (this.form.invalid) {
      return;
    }

    var updateRequest = {
      firstname: this.form.value.firstname,
      lastname: this.form.value.lastname,
      email: this.form.value.email,
      gender: ""+this.form.value.gender.id,
      companyname: this.form.value.company,
      country: this.form.value.country
    };

    this.loading = true;
    this.accountService.update(updateRequest) /*this.form.value)*/
      .pipe(first())
      .subscribe(
        data => {
          this.alertService.success('Ditt konto har uppdaterats', { keepAfterRouteChange: true });
          this.loading = false;
        },
        error => {
          const err = error.error.message || error.statusText;
          this.alertService.error(err);
          this.loading = false;
        });
  }
}
