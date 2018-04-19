import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';

import { ReportingRoutingModule } from './reporting-routing.module';



@NgModule({
  imports: [
    SharedModule,
    ReportingRoutingModule
  ],
  declarations: []
})
export class ReportingModule { }
