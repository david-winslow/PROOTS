



import { NgModule, ErrorHandler } from "@angular/core";
import { RouterModule } from "@angular/router";
import { HttpClientModule } from '@angular/common/http';

import { TranslateModule, TranslateLoader } from "@ngx-translate/core";
import { ToastyModule } from 'ng2-toasty';
import { ChartsModule } from 'ng2-charts';
import { NgxCarouselModule } from 'ngx-carousel';

import { AppRoutingModule } from './app-routing.module';
import { AppErrorHandler } from './app-error.handler';

import { SharedModule } from './shared/shared.module'
import { AdminModule } from './admin/admin.module';
import { ReportingModule } from './reporting/reporting.module';
import { SettingsModule } from './settings/settings.module';
import { FooterModule } from './shared/footer.component';
import { ThemePickerModule } from './shared/theme-picker.component';

import { AppTitleService } from './services/app-title.service';
import { AppTranslationService, TranslateLanguageLoader } from './services/app-translation.service';
import { ConfigurationService } from './services/configuration.service';
import { AlertService } from './services/alert.service';
import { LocalStoreManager } from './services/local-store-manager.service';
import { EndpointFactory } from './services/endpoint-factory.service';
import { NotificationService } from './services/notification.service';
import { NotificationEndpoint } from './services/notification-endpoint.service';
import { AccountService } from './services/account.service';
import { AccountEndpoint } from './services/account-endpoint.service';

import { AppComponent } from "./app.component";
import { LoginComponent } from "./components/login/login.component";
import { LoginControlComponent } from "./components/login/login-control.component";
import { LoginDialogComponent } from "./components/login/login-dialog.component";
import { HomeComponent } from "./components/home/home.component";
import { CustomersComponent } from "./components/customers/customers.component";
import { AboutComponent } from "./components/about/about.component";
import { NotFoundComponent } from "./components/not-found/not-found.component";

import { BannerDemoComponent } from "./components/controls/banner-demo.component";
import { TodoDemoComponent } from "./components/controls/todo-demo.component";
import { StatisticsDemoComponent } from "./components/controls/statistics-demo.component";
import { NotificationsViewerComponent } from "./components/controls/notifications-viewer.component";
import { AddTaskDialogComponent } from './components/controls/add-task-dialog.component';

@NgModule({
    imports: [
        SharedModule,
        FooterModule,
        ThemePickerModule,
        HttpClientModule,
        AdminModule,
        ReportingModule,
        SettingsModule,
        AppRoutingModule,
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useClass: TranslateLanguageLoader
            }
        }),
        ToastyModule.forRoot(),
        ChartsModule,
        NgxCarouselModule
    ],
    declarations: [
        AppComponent,
        LoginComponent, LoginControlComponent, LoginDialogComponent,
        HomeComponent,
        CustomersComponent,
        AboutComponent,
        NotFoundComponent,
        NotificationsViewerComponent,
        AddTaskDialogComponent,
        StatisticsDemoComponent, TodoDemoComponent, BannerDemoComponent
    ],
    providers: [
        { provide: 'BASE_URL', useFactory: getBaseUrl },
        { provide: ErrorHandler, useClass: AppErrorHandler },
        AlertService,
        ConfigurationService,
        AppTitleService,
        AppTranslationService,
        NotificationService,
        NotificationEndpoint,
        AccountService,
        AccountEndpoint,
        LocalStoreManager,
        EndpointFactory
    ],
    entryComponents: [
        LoginDialogComponent,
        AddTaskDialogComponent
    ],
    bootstrap: [AppComponent]
})
export class AppModule {
}

export function getBaseUrl() {
    return document.getElementsByTagName('base')[0].href;
}
