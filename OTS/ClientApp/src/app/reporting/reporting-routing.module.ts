import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";

import { ReportComponent } from "./report.component";

import { AuthService } from "../services/auth.service";
import { AuthGuard } from "../services/auth-guard.service";

const reportingRoutes: Routes = [
  {
    path : "report",
    component : ReportComponent
   }
];

@NgModule({
  imports: [RouterModule.forChild(reportingRoutes)],
  exports: [RouterModule]
})

export class ReportingRoutingModule { }
