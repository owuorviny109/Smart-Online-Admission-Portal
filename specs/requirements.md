# Smart Online Admission Portal (SOAP) - Requirements

## Introduction

The Smart Online Admission Portal (SOAP) bridges the gap between Kenya's NEMIS placement system and actual school onboarding. After the Ministry places students in schools, parents still need to physically report with documents, fill manual forms, and wait in long queues. SOAP digitizes this post-placement process, allowing parents to complete admission requirements online before reporting day.

## Requirements

### Requirement 1

**User Story:** As a parent, I want to complete my child's admission form online using their NEMIS admission number, so that I can avoid long queues on reporting day.

#### Acceptance Criteria

1. WHEN I enter my child's NEMIS admission number THEN the system SHALL verify it and display basic placement information
2. WHEN I fill out the student bio-data form THEN the system SHALL validate required fields (name, age, parent contacts, emergency contacts)
3. WHEN I submit the completed form THEN the system SHALL save the information and send me a confirmation SMS
4. IF the NEMIS number is invalid THEN the system SHALL display an error message with instructions

### Requirement 2

**User Story:** As a parent, I want to upload my child's required documents online, so that school staff can verify them before reporting day.

#### Acceptance Criteria

1. WHEN I access the document upload section THEN the system SHALL show a checklist of required documents (KCPE slip, birth certificate, medical form)
2. WHEN I upload a document THEN the system SHALL accept PDF/JPEG files up to 2MB and display upload confirmation
3. WHEN all documents are uploaded THEN the system SHALL mark my application as "Documents Complete"
4. IF a document is unclear or invalid THEN the system SHALL allow me to re-upload with feedback

### Requirement 3

**User Story:** As a school administrator, I want to review student applications in a dashboard, so that I can approve them efficiently before reporting day.

#### Acceptance Criteria

1. WHEN I log into the school dashboard THEN the system SHALL display all applications with status (pending, approved, incomplete)
2. WHEN I click on an application THEN the system SHALL show student details and uploaded documents
3. WHEN I approve an application THEN the system SHALL generate an admission slip and notify the parent via SMS
4. IF documents are missing or unclear THEN the system SHALL allow me to request corrections with comments

### Requirement 4

**User Story:** As a student, I want to receive a digital admission slip after approval, so that I can check in quickly on reporting day.

#### Acceptance Criteria

1. WHEN my application is approved THEN the system SHALL generate a unique admission slip with my details and a verification code
2. WHEN I arrive at school THEN staff SHALL be able to verify my admission using the code
3. WHEN my code is verified THEN the system SHALL display my profile and mark me as "Checked In"
4. IF I lose my admission slip THEN the system SHALL allow me to retrieve it using my phone number

### Requirement 5

**User Story:** As a school principal, I want to see basic statistics about the admission process, so that I can plan for reporting day.

#### Acceptance Criteria

1. WHEN I access the reports section THEN the system SHALL show total applications, approved count, and pending count
2. WHEN I view daily progress THEN the system SHALL display applications completed per day leading to reporting day
3. WHEN reporting day approaches THEN the system SHALL show expected student arrival numbers
4. IF there are bottlenecks THEN the system SHALL highlight areas needing attention

### Requirement 6

**User Story:** As a parent with limited internet access, I want to complete basic information via SMS, so that I can participate even without reliable internet.

#### Acceptance Criteria

1. WHEN I send my child's NEMIS number via SMS to the system THEN it SHALL respond with a step-by-step form
2. WHEN I reply with required information THEN the system SHALL save each response and prompt for the next field
3. WHEN I complete the SMS form THEN the system SHALL confirm completion and provide next steps
4. IF I need to upload documents THEN the system SHALL direct me to the nearest cyber cafe or school office