import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

import { AccountService } from '@app/_services';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(
    private router: Router,
    private accountService: AccountService
  ) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
    const currentUser = this.accountService.currentUserValue;
    if (currentUser) {
      console.log('user is logged in according to AuthGuard');
      // logged in so return true
      return true;
    }

    // not logged in so redirect to login page
    this.router.navigate(['/login'], {});
    //todo: fix:
    // not logged in so redirect to login page with the return url
    // this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    // login page always redirects to 'mina-sida', otherwise data is not fetched correctly from the api 
    return false;
  }
}
