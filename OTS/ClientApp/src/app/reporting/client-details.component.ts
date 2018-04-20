import { Component, OnInit, OnChanges, OnDestroy, ViewChild, SimpleChanges, Input } from "@angular/core";
import { NgForm, FormGroup, FormBuilder, Validators } from "@angular/forms";

@Component({
  selector: "app-client-details",
  templateUrl: "./client-details.component.html",
  styleUrls: ["./client-details.component.scss"]
})
export class ClientDetailsComponent implements OnChanges, OnDestroy {


  @ViewChild("form")
  private form: NgForm;
  clientDetailsFormGroup: FormGroup;
  @Input()
  isEditMode = false;

  ngOnDestroy(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
  }
  constructor(
    private formBuilder: FormBuilder
) {
  this.buildForm();
}

public beginEdit() {
  this.isEditMode = true;
  return;
}
public save() {return; }

private buildForm() {
  this.clientDetailsFormGroup = this.formBuilder.group({
      clientName: '',
      homeLanguage: ''});
  }
  get floatLabels(): string {
    return this.isEditMode ? 'auto' : 'always';
}
}
