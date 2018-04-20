import { Component, OnInit, AfterViewInit, OnDestroy, ViewChild } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";
import { MatExpansionPanel } from "@angular/material";

import "rxjs/add/operator/switchMap";

import { ClientDetailsComponent } from "./client-details.component";

@Component({
    selector: "app-report",
    templateUrl: "./report.component.html",
    styleUrls: ["./report.component.scss"],
    animations: [fadeInOut]
})
export class ReportComponent {
@ViewChild(ClientDetailsComponent)
clientDetailsPanel: MatExpansionPanel;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
) {}

  public navigateToFragment(fragment: string) {
    if (fragment) {
        this.router.navigateByUrl(`/report#${fragment}`);
    }
}
}

