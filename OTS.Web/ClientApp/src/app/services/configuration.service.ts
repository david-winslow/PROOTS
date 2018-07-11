import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { LocalStoreManager } from './local-store-manager.service';
import { DBkeys } from './db-Keys';
import { Utilities } from './utilities';
import { environment } from '../../environments/environment';

type UserConfiguration = {
    language: string,
    homeUrl: string,
    themeId: number,
    showDashboardNotifications: boolean,
};

@Injectable()
export class ConfigurationService {
    public static readonly appVersion: string = "1.2.0";

    public baseUrl = environment.baseUrl || Utilities.baseUrl();
    public tokenUrl = environment.tokenUrl || environment.baseUrl || Utilities.baseUrl();
    public loginUrl = environment.loginUrl;
    public fallbackBaseUrl = "http://OTS-pro.ebenmonney.com";

    public static readonly defaultLanguage: string = "en";
    public static readonly defaultHomeUrl: string = "/";
    public static readonly defaultThemeId: number = 1;
    public static readonly defaultShowDashboardNotifications: boolean = true;

    private _language: string = null;
    private _homeUrl: string = null;
    private _themeId: number = null;
    private _showDashboardNotifications: boolean = null;
    private onConfigurationImported: Subject<boolean> = new Subject<boolean>();

    configurationImported$ = this.onConfigurationImported.asObservable();

    constructor(
        private localStorage: LocalStoreManager,
    ) {
        this.loadLocalChanges();
    }

    private loadLocalChanges() {
        if (this.localStorage.exists(DBkeys.LANGUAGE)) {
            this._language = this.localStorage.getDataObject<string>(DBkeys.LANGUAGE);
        }

        if (this.localStorage.exists(DBkeys.HOME_URL)) {
            this._homeUrl = this.localStorage.getDataObject<string>(DBkeys.HOME_URL);
        }

        if (this.localStorage.exists(DBkeys.THEME_ID)) {
            this._themeId = this.localStorage.getDataObject<number>(DBkeys.THEME_ID);
        }



        if (this.localStorage.exists(DBkeys.SHOW_DASHBOARD_NOTIFICATIONS)) {
            this._showDashboardNotifications =
                this.localStorage.getDataObject<boolean>(DBkeys.SHOW_DASHBOARD_NOTIFICATIONS);
        }


    }

    private saveToLocalStore(data: any, key: string) {
        setTimeout(() => this.localStorage.savePermanentData(data, key));
    }

    public import(jsonValue: string) {
        this.clearLocalChanges();

        if (jsonValue) {


            let importValue: UserConfiguration = Utilities.JsonTryParse(jsonValue);



            if (importValue.homeUrl != null) {
                this.homeUrl = importValue.homeUrl;
            }

            if (importValue.themeId != null) {
                this.themeId = importValue.themeId;
            }



            if (importValue.showDashboardNotifications != null) {
                this.showDashboardNotifications = importValue.showDashboardNotifications;
            }


        }

        this.onConfigurationImported.next();
    }

    public export(changesOnly = true): string {
        let exportValue: UserConfiguration =
        {
            language: changesOnly ? this._language : this._language,
            homeUrl: changesOnly ? this._homeUrl : this.homeUrl,
            themeId: changesOnly ? this._themeId : this.themeId,
            showDashboardNotifications:
                changesOnly ? this._showDashboardNotifications : this.showDashboardNotifications

        };

        return JSON.stringify(exportValue);
    }

    public clearLocalChanges() {
        this._language = null;
        this._homeUrl = null;
        this._themeId = null;
        this._showDashboardNotifications = null;
        this.localStorage.deleteData(DBkeys.LANGUAGE);
        this.localStorage.deleteData(DBkeys.HOME_URL);
        this.localStorage.deleteData(DBkeys.THEME_ID);
        this.localStorage.deleteData(DBkeys.SHOW_DASHBOARD_STATISTICS);
        this.localStorage.deleteData(DBkeys.SHOW_DASHBOARD_NOTIFICATIONS);
        this.localStorage.deleteData(DBkeys.SHOW_DASHBOARD_TODO);
        this.localStorage.deleteData(DBkeys.SHOW_DASHBOARD_BANNER);

    }
    set homeUrl(value: string) {
        this._homeUrl = value;
        this.saveToLocalStore(value, DBkeys.HOME_URL);
    }

    get homeUrl() {
        return this._homeUrl || ConfigurationService.defaultHomeUrl;
    }

    set themeId(value: number) {
        this._themeId = value;
        this.saveToLocalStore(value, DBkeys.THEME_ID);
    }

    get themeId() {
        return this._themeId || ConfigurationService.defaultThemeId;
    }

    set showDashboardNotifications(value: boolean) {
        this._showDashboardNotifications = value;
        this.saveToLocalStore(value, DBkeys.SHOW_DASHBOARD_NOTIFICATIONS);
    }

    get showDashboardNotifications() {
        return this._showDashboardNotifications != null
            ? this._showDashboardNotifications
            : ConfigurationService.defaultShowDashboardNotifications;
    }
}
