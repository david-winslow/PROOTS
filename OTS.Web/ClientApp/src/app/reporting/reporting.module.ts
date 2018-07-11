import { NgModule } from "@angular/core";
import { SharedModule } from "../shared/shared.module";


import { ReportComponent } from "./report.component";
import { ClientDetailsComponent } from './client-details.component';
import { ReportDetailsComponent } from './report-details.component';



@NgModule({
  imports: [
    SharedModule
  ],
  declarations:
   [ReportComponent, ClientDetailsComponent, ReportDetailsComponent]
})
export class ReportingModule { }
