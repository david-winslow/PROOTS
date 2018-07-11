import { Component, OnInit, AfterViewInit, OnDestroy, ViewChild } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";
import { MatExpansionPanel } from "@angular/material";

import "rxjs/add/operator/switchMap";

import { ClientDetailsComponent } from "./client-details.component";
import { fadeInOut } from "../services/animations";
import { ReportDetailsComponent } from "./report-details.component";

@Component({
    selector: "app-report",
    templateUrl: "./report.component.html",
    styleUrls: ["./report.component.scss"],
    animations: [fadeInOut]
})
export class ReportComponent {
    @ViewChild(ClientDetailsComponent)
    private clientDetails: ClientDetailsComponent;
    @ViewChild("clientDetailsPanel")
    private clientDetailsPanel: MatExpansionPanel;

    @ViewChild(ReportDetailsComponent)
    private reportDetails: ReportDetailsComponent;
    @ViewChild("reportDetailsPanel")
    private reportDetailsPanel: MatExpansionPanel;



    constructor(private router: Router, private route: ActivatedRoute) {
    }

    public navigateToFragment(fragment: string) {
        if (fragment) {
            this.router.navigateByUrl(`/report#${fragment}`);
        }
    }
    public save(panelName: string){
        if (this[panelName].save())
        {
            this[panelName + "Panel"].close();
        }
    }

    public cancel(panelName: string) {
        this[panelName].cancel();
        this[panelName].isComplete = false;
        this[panelName + "Panel"].close();
    }

    public beginEdit(panelName: string) {
        this[panelName].beginEdit();
    }

    public isEditMode(panelName: string): Boolean {
        return  this[panelName].isEditMode;
    }

    public isComplete(panelName: string): Boolean{
        return this[panelName].complete;
    }
}
