
import {throwError as observableThrowError,  Observable ,  Subject } from 'rxjs';




import { Injectable, Injector } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';





import { AuthService } from './auth.service';
import { ConfigurationService } from './configuration.service';
import { catchError, mergeMap } from 'rxjs/operators';

@Injectable()
export class EndpointFactory {
    static readonly apiVersion: string = "1";

    private readonly _loginUrl: string = "/connect/token";

    private get loginUrl() { return this.configurations.tokenUrl + this._loginUrl; }

    private taskPauser: Subject<any>;
    private isRefreshingLogin: boolean;

    private _authService: AuthService;

    private get authService() {
        if (!this._authService) {
            this._authService = this.injector.get(AuthService);
        }

        return this._authService;
    }

    constructor(protected http: HttpClient,
        protected configurations: ConfigurationService,
        private injector: Injector) {
    }

    getLoginEndpoint<T>(userName: string, password: string): Observable<T> {

        let header = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });

        let params = new HttpParams()
            .append('username', userName)
            .append('password', password)
            .append('client_id', 'OTS_spa')
            .append('grant_type', 'password')
            .append('scope', 'openid email phone profile offline_access roles OTS_api');

        return this.http.post<T>(this.loginUrl, params, { headers: header });
    }

    getRefreshLoginEndpoint<T>(): Observable<T> {
        let header = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });

        let params = new HttpParams()
            .append('refresh_token', this.authService.refreshToken)
            .append('client_id', 'OTS_spa')
            .append('grant_type', 'refresh_token');

        return this.http.post<T>(this.loginUrl, params, { headers: header })
            .pipe(catchError(error => {
                return this.handleError(error, () => this.getRefreshLoginEndpoint());
            })) as Observable<T>;
    }

    protected getRequestHeaders(): { headers: HttpHeaders | { [header: string]: string | string[]; } } {
        let headers = new HttpHeaders({
            'Authorization': 'Bearer ' + this.authService.accessToken,
            'Content-Type': 'application/json',
            'Accept': `application/vnd.iman.v${EndpointFactory.apiVersion}+json, application/json, text/plain, */*`,
            'App-Version': ConfigurationService.appVersion
        });

        return { headers: headers };
    }

    protected handleError(error, continuation: () => Observable<any>) {
        if (error.status == 401) {
            if (this.isRefreshingLogin) {
                return this.pauseTask(continuation);
            }

            this.isRefreshingLogin = true;

            return this.authService.refreshLogin()
                .pipe(mergeMap(data => {
                    this.isRefreshingLogin = false;
                    this.resumeTasks(true);

                    return continuation();
                }))
                .pipe(catchError(refreshLoginError => {
                    this.isRefreshingLogin = false;
                    this.resumeTasks(false);

                    if (refreshLoginError.status == 401 ||
                    (refreshLoginError.url &&
                        refreshLoginError.url.toLowerCase().includes(this.loginUrl.toLowerCase()))) {
                        this.authService.reLogin();
                        return observableThrowError('session expired');
                    } else {
                        return observableThrowError(refreshLoginError || 'server error');
                    }
                }));
        }

        if (error.url && error.url.toLowerCase().includes(this.loginUrl.toLowerCase())) {
            this.authService.reLogin();

            return observableThrowError((error.error && error.error.error_description)
                ? `session expired (${error.error.error_description})`
                : 'session expired');
        } else {
            return observableThrowError(error);
        }
    }

    private pauseTask(continuation: () => Observable<any>) {
        if (!this.taskPauser) {
            this.taskPauser = new Subject();
        }

        return this.taskPauser.switchMap(continueOp => {
            return continueOp ? continuation() : observableThrowError('session expired');
        });
    }

    private resumeTasks(continueOp: boolean) {
        setTimeout(() => {
            if (this.taskPauser) {
                this.taskPauser.next(continueOp);
                this.taskPauser.complete();
                this.taskPauser = null;
            }
        });
    }
}
