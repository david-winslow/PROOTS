import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ReportingRoutingModule } from './reporting-routing.module';
import { NewComponent } from './new/new.component';
import { NewReportComponent } from '../new-report/new-report.component';

@NgModule({
  imports: [
    CommonModule,
    ReportingRoutingModule
  ],
  declarations: [NewComponent, NewReportComponent]
})
export class ReportingModule { }
