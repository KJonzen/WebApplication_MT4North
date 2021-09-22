import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import { environment } from '@environments/environment';
import { User, UserAuth } from '@app/_models';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private currentUserSubject: BehaviorSubject<User>;
  public currentUser: Observable<User>;

  private currentUserAuthSubject: BehaviorSubject<UserAuth>;
  public currentUserAuth: Observable<UserAuth>;

  constructor(private http: HttpClient) {
    this.currentUserSubject = new BehaviorSubject<User>(JSON.parse(localStorage.getItem('currentUser')));
    this.currentUser = this.currentUserSubject.asObservable();

    this.currentUserAuthSubject = new BehaviorSubject<UserAuth>(JSON.parse(localStorage.getItem('currentUserAuth')));
    this.currentUserAuth = this.currentUserAuthSubject.asObservable();

  }

  public get currentUserValue(): User {
        return this.currentUserSubject.value;
  }

  public get currentUserAuthValue(): UserAuth {
    return this.currentUserAuthSubject.value;
  }
  
    login(email: string, password: string) {
          return this.http.post<any>(`${environment.apiUrl}/Account/login`, { email, password })
              .pipe(map(user => {
                // store user details and jwt token in local storage to keep user logged in between page refreshes
                localStorage.setItem('currentUserAuth', JSON.stringify(user));
                this.currentUserAuthSubject.next(user);

                localStorage.setItem('currentUser', JSON.stringify(user));
                this.currentUserSubject.next(user);
                this.startRefreshTokenTimer();
                return user;
              }));
      }

    logout() {
        // remove user from local storage to log user out
      localStorage.removeItem('currentUserAuth');
      localStorage.removeItem('currentUser');
      this.stopRefreshTokenTimer();

      this.currentUserAuthSubject.next(null);
      this.currentUserSubject.next(null);
    }

    register(user: User) {
      return this.http.post(`${environment.apiUrl}/Account/register`, user);
    }

    getAll() {
      return this.http.get<User[]>(`${environment.apiUrl}/users`);
    } 

  getCurrentUser() {
    return this.http.get<User>(`${environment.apiUrl}/Account/user`).pipe(map(user => {
      // store user details and jwt token in local storage to keep user logged in between page refreshes
      // why?
      if (user.gender == "null") {
        user.gender = null;
      }
      localStorage.setItem('currentUser', JSON.stringify(user));
      this.currentUserSubject.next(user);
      return user;
    }));
  }

    getById(id: string) {
      return this.http.get<User>(`${environment.apiUrl}/users/${id}`).pipe(map(user => {
        // store user details and jwt token in local storage to keep user logged in between page refreshes
        // why?
        if (user.gender == "null") {
          user.gender = null;
        }
        localStorage.setItem('currentUser', JSON.stringify(user));
        this.currentUserSubject.next(user);
        return user;
      }));
  }

  update(params) {
    return this.http.put(`${environment.apiUrl}/Account/user`, params)
      .pipe(map(x => {
          {
          // update local storage
          const user = { ...this.currentUserValue, ...params };
          localStorage.setItem('currentUser', JSON.stringify(user));
          console.log('USER: ', user);
          // publish updated user to subscribers
          this.currentUserSubject.next(user);
        }
        return x;
      }));
  }

  delete(): Observable<any> {
    return this.http.delete(`${environment.apiUrl}/Account/user`, { observe: 'response' })
      .pipe(
        tap(
          response => {
            this.logout();
          },
          error => {
            // something went wrong
            return throwError(error);
          }
        )
      );
  }

  refreshToken() {
    return this.http.post<any>(`${environment.apiUrl}/Account/refresh-token`, { refreshToken: this.currentUserAuthValue.refreshToken}, { withCredentials: true }) 
      .pipe(map((user) => {
        console.log('refreshToken response:' + user)
        localStorage.setItem('currentUserAuth', JSON.stringify(user));
        this.currentUserAuthSubject.next(user);
        //this.currentUserSubject.next(user);
        this.startRefreshTokenTimer();
        return user;
      }));
  }

  // helper methods

  private refreshTokenTimeout;

  private startRefreshTokenTimer() {
    // parse json object from base64 encoded jwt token
    const jwtToken = JSON.parse(atob(this.currentUserAuthValue.accessToken.split('.')[1]));
    // set a timeout to refresh the token a minute before it expires
    const expires = new Date(jwtToken.exp * 1000);
    const timeout = expires.getTime() - Date.now() - (60 * 1000);
    // console.log('refresh-token expires: ' + expires);
    // console.log('refresh-token timeout: ' + timeout);
    // 
    this.refreshTokenTimeout = setTimeout(() => this.refreshToken().subscribe(), timeout);
  }

  private stopRefreshTokenTimer() {
    clearTimeout(this.refreshTokenTimeout);
  }

    //update(id, params) {
    //  return this.http.put(`${environment.apiUrl}/users/${id}`, params)
    //    .pipe(map(x => {
    //      // update stored user if the logged in user updated their own record
    //      if (id == this.userValue.id) {
    //        // update local storage
    //        const user = { ...this.userValue, ...params };
    //        localStorage.setItem('user', JSON.stringify(user));

    //        // publish updated user to subscribers
    //        this.userSubject.next(user);
    //      }
    //      return x;
    //    }));
    //}

    //delete(id: string) {
    //  return this.http.delete(`${environment.apiUrl}/users/${id}`)
    //    .pipe(map(x => {
    //      // auto logout if the logged in user deleted their own record
    //      if (id == this.userValue.id) {
    //        this.logout();
    //      }
    //      return x;
    //    }));
    //}
}
