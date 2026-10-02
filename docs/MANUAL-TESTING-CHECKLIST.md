
# Manual Testing Checklist – Online Insurance Management System

## Purpose
This checklist documents manual test scenarios for verifying the supported workflows of the Online Insurance Management System.

## 1. User Authentication
- [ ] Open the login page and enter valid credentials.
  - Expected result: The user is authenticated and redirected to the appropriate page.
- [ ] Enter an incorrect password.
  - Expected result: Login is rejected and an appropriate error message is displayed.

## 2. Insurance Policy Management
- [ ] Open the policy section using an authorized account.
  - Expected result: Available policy information is displayed.
- [ ] Open a policy details page.
  - Expected result: The selected policy details are displayed correctly.

## 3. Claims Processing
- [ ] Open the claims section.
  - Expected result: The user can access the claims functionality permitted by their role.
- [ ] Submit a claim using valid required information, if claim submission is available.
  - Expected result: The application accepts the submission and displays a confirmation or updated claim status.

## 4. Loan Management
- [ ] Open the loan section using an authorized account.
  - Expected result: The loan information or available loan actions are displayed.
- [ ] Review a loan record.
  - Expected result: The relevant loan details and status are displayed.

## 5. Customer and Agent Portals
- [ ] Sign in with a customer account and open the customer portal.
  - Expected result: Customer-specific information and permitted actions are displayed.
- [ ] Sign in with an agent account and open the agent portal.
  - Expected result: Agent-specific information and permitted actions are displayed.

## 6. Admin Dashboard
- [ ] Sign in with an administrator account.
  - Expected result: The admin dashboard is accessible.
- [ ] Open an available management section.
  - Expected result: The relevant management page loads and displays its information.

## 7. Email Notifications
- [ ] Perform an application action that triggers an email, if configured.
  - Expected result: The application reports the notification outcome, and the configured recipient receives the expected email.

## Test Notes
- Execute each scenario in the running application.
- Record the actual result and mark each test as Pass or Fail.
- Update scenarios if a feature or workflow differs from the current implementation.
- Do not mark tests as passed unless they have been executed.
