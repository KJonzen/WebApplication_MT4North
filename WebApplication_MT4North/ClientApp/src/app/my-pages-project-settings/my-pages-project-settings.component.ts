import { Component, OnDestroy } from '@angular/core';
import { AlertService, AccountService, ProjectService } from '@app/_services';
import { User, UserProject, Project, ProjectRights, ProjectRole } from '../_models';
import { first } from 'rxjs/operators';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';

@Component({
  selector: 'app-footer-component',
  templateUrl: './my-pages-project-settings.component.html',
  styleUrls: ['./my-pages-project-settings.component.css']
})
export class MyPagesProjectSettingsComponent implements OnDestroy {
  form: FormGroup;
  loading = false;
  submitted = false;
  hasRights = false;
  currentUser: User;
  userProjects: UserProject[];
  selectedProject: Project;
  error = '';
  accountSubscription: Subscription;
  userProjectsSubscription: Subscription;
  selectedProjectSubscription: Subscription;
  editModeIsOn = false;

  constructor(
    private alertService: AlertService,
    private accountService: AccountService,
    private projectService: ProjectService,
    private formBuilder: FormBuilder,
    private router: Router
  ) {
    this.accountService.getCurrentUser();

    this.accountSubscription = this.accountService.currentUser.subscribe(x => { this.currentUser = x; });

    //this.projectService.getUserProjects(this.projectService.selectedProjectValue.projectid).subscribe(x => {
    //  console.log('XYZ: ', x);
    //});;
    this.selectedProjectSubscription = this.projectService.selectedProject.subscribe(x => { this.selectedProject = x; });

    this.userProjectsSubscription = this.projectService.userProjects.subscribe(x => {
      console.log('X: ', x);
      this.userProjects = x;
      let user = this.userProjects.filter(x => x.userid == this.currentUser.id)[0];
      this.hasRights = user.rights == ProjectRights.READWRITE && user.role == ProjectRole.OWNER;
    });

    //console.log('**currentUser: ', this.currentUser);
    //console.log('**userprojects: ', this.userProjects);

    //console.log('userProject: ', this.userProjects.filter(x => x.userprojectid == this.currentUser.id));

  }

  ngOnInit() {
    this.form = this.formBuilder.group({
      projectid: [this.selectedProject.projectid],
      name: [this.selectedProject.name, Validators.required],
      description: [this.selectedProject.description, Validators.required],
    });
  }

  ngOnDestroy() {
    console.log('unsubscribing from observers in project settings');
    this.accountSubscription.unsubscribe();
    this.userProjectsSubscription.unsubscribe();
    this.selectedProjectSubscription.unsubscribe();

  }

  activateEditMode(param: boolean) {
    this.editModeIsOn = param;
    console.log('edit mode on? ', param);
  }

  // convenience getter for easy access to form fields
  get f() { return this.form.controls; }

  onSubmit() {
    console.log('form submitted');
    this.submitted = true;

    // reset alerts on submit
    this.alertService.clear();

    // stop here if form is invalid
    if (this.form.invalid) {
      return;
    }
    console.log('form value: ', this.form.value);

    this.loading = true;
    this.projectService.updateProject(this.form.value)
      .pipe(first())
      .subscribe(
        data => {
          this.alertService.success('Projektet har uppdaterats', { keepAfterRouteChange: true });
          this.loading = false;
          this.editModeIsOn = false;
        },
        error => {
          const err = error.error.message || error.statusText;
          this.alertService.error(err);
          this.loading = false;
        });
  }

  deleteProject(projectId: string) {
    this.projectService.deleteProject(projectId)
      .pipe(first())
      .subscribe(
        data => {
          this.alertService.success('Projektet har tagits bort', { keepAfterRouteChange: true });
          // redirect to 'mina-sidor'
          this.router.navigate(['/my-pages/start']);
        },
        error => {
          if (error.status == 403) {
            this.alertService.error('Otillåtet. Du måste vara projektägare för att ta bort projektet');
          }
          if (error.status == 404) {
            this.alertService.error('Projektet hittades inte. Försök igen senare');
          }
        }
      );
  }

  resetProject(projectId: string) {
    this.projectService.resetProject(projectId)
      .pipe(first())
      .subscribe(
        data => {
          this.alertService.success('Projektet har nollställts', { keepAfterRouteChange: true });
          // redirect to 'start page of the project'
          this.router.navigate([`/my-pages/projects/${projectId}/activity-status`]);
        },
        error => {
          if (error.status == 403) {
            this.alertService.error('Otillåtet. Du måste vara projektägare för att nollställa projektet');
          }
          if (error.status == 404) {
            this.alertService.error('Projektet hittades inte. Försök igen senare');
          }
        }
      );
  }

}
