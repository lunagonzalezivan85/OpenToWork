---
description: How the company CRM pipeline handles meeting dates and plan selection
---
# Company CRM Pipeline — Meeting Date & Plan Selection

Use this skill when modifying the company pipeline feature (`PTCompanyPipeline`, `CompanyCrmService`, `PipelineDetail.razor`).

## Entity Model

- `PTCompanyPipeline` has `DateTime? MeetingDate`.
- Migration `20260914045847_AddMeetingDate` added the column.

## DTOs

- `CompanyMoveStageDto.MeetingDate`
- `CompanyPipelineDetailDto.MeetingDate`

## Service Behavior

- `CompanyCrmService.MoveStageAsync` saves `MeetingDate` when present.
- `CompanyCrmService.GetPipelineDetailAsync` maps `MeetingDate` to the DTO.

## Stage Advance Modal (`PipelineDetail.razor`)

| From | To | Required Fields | Persistence |
|---|---|---|---|
| Lead | Contacted | Contact method (dropdown) + comments | Notes prefixed with `[METODO: ...]` |
| Contacted | Meeting Scheduled | Meeting date (`datetime-local`) + comments | `MeetingDate` entity property |
| Meeting Scheduled | Proposal Sent | Plan/price selection (dropdown) + comments | Notes prefixed with `[PLAN: ...]` and `SelectedPlanId` |

## UI Notes

- Show a banner with the meeting date while in the **Meeting Scheduled** stage.
- Use `value` + `@oninput` with `OnMeetingDateChanged` for `datetime-local` inputs (`@bind` resets the value).

## API

- `PUT api/admin/company-crm/pipeline/{pipelineId}/notes` updates notes.
- `AdminAuthApiService.UpdateCompanyPipelineNotesAsync` is the client method.

## Key Files

- `PTCompanyPipeline.cs`
- `CompanyCrmDtos.cs`
- `CompanyCrmService.cs`
- `ICompanyCrmService.cs`
- `CompanyCrmController.cs`
- `AdminAuthApiService.cs`
- `PipelineDetail.razor`
- `20260914045847_AddMeetingDate.cs`
