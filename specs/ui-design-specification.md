# SOAP Platform - UI Design Specification

## Design Brief for Designer/AI Tools

**Project:** Smart Online Admission Portal (SOAP) for Kenyan Secondary Schools
**Target Users:** Parents/Students (primary), School Administrators (secondary)
**Platform:** Web application (desktop + mobile responsive)
**Design Style:** Clean, professional, trustworthy, accessible

---

## 1. Design System Foundation

### Color Palette
```
Primary Colors:
- Education Blue: #1e40af (Main brand color, buttons, headers)
- Success Green: #059669 (Approved status, success messages)
- Warning Orange: #d97706 (Pending status, attention needed)
- Error Red: #dc2626 (Rejected status, error messages)

Neutral Colors:
- White: #ffffff (Card backgrounds, main content areas)
- Light Gray: #f8fafc (Page backgrounds, subtle sections)
- Medium Gray: #64748b (Secondary text, placeholders)
- Dark Gray: #1e293b (Primary text, headings)

Status Colors:
- Approved: #10b981 (Green with checkmark ✅)
- Pending: #f59e0b (Orange with clock ⏳)
- Rejected: #ef4444 (Red with X ❌)
- Missing: #6b7280 (Gray with warning ⚠️)
```

### Typography
```
Primary Font: Inter (Google Fonts)
- Headings: Inter Bold (600-700 weight)
- Body Text: Inter Regular (400 weight)
- Small Text: Inter Medium (500 weight)
- Numbers/Codes: JetBrains Mono (for KCPE numbers)

Font Sizes:
- H1 (Page Titles): 32px (mobile: 24px)
- H2 (Section Headers): 24px (mobile: 20px)
- H3 (Card Titles): 18px (mobile: 16px)
- Body Text: 16px (mobile: 14px)
- Small Text: 14px (mobile: 12px)
```

### Spacing & Layout
```
Container Max Width: 1200px
Grid System: 12-column Bootstrap-style grid
Spacing Scale: 8px base unit (8px, 16px, 24px, 32px, 48px, 64px)
Border Radius: 8px (cards), 4px (buttons, inputs)
Box Shadow: 0 1px 3px rgba(0,0,0,0.1) (subtle elevation)
```

---

## 2. Component Library Specifications

### Buttons
```
Primary Button:
- Background: #1e40af (Education Blue)
- Text: White, Inter Medium, 16px
- Padding: 12px 24px
- Border Radius: 8px
- Hover: Darken by 10%
- Example: [VERIFY], [SUBMIT APPLICATION]

Secondary Button:
- Background: Transparent
- Border: 2px solid #1e40af
- Text: #1e40af, Inter Medium, 16px
- Padding: 10px 22px
- Hover: Background #1e40af, Text white
- Example: [SAVE DRAFT], [CANCEL]

Success Button:
- Background: #059669 (Success Green)
- Text: White, Inter Medium, 16px
- Example: [APPROVE], [CONFIRM]

Danger Button:
- Background: #dc2626 (Error Red)
- Text: White, Inter Medium, 16px
- Example: [REJECT], [DELETE]
```

### Form Elements
```
Input Fields:
- Border: 1px solid #d1d5db
- Border Radius: 6px
- Padding: 12px 16px
- Font: Inter Regular, 16px
- Focus: Border #1e40af, Box shadow blue
- Placeholder: #9ca3af

Select Dropdowns:
- Same styling as inputs
- Arrow icon: Chevron down
- Options: White background, hover #f3f4f6

File Upload Areas:
- Dashed border: 2px dashed #d1d5db
- Background: #f9fafb
- Padding: 32px
- Center-aligned content
- Drag hover: Border #1e40af, Background #eff6ff
```

### Status Indicators
```
Status Badges:
- Approved: Green background #dcfce7, Green text #166534, ✅ icon
- Pending: Orange background #fef3c7, Orange text #92400e, ⏳ icon
- Rejected: Red background #fee2e2, Red text #991b1b, ❌ icon
- Missing: Gray background #f3f4f6, Gray text #374151, ⚠️ icon

Progress Bar:
- Background: #e5e7eb
- Fill: #1e40af (Education Blue)
- Height: 8px
- Border Radius: 4px
- Text: Inter Medium, 14px, above bar
```

---

## 3. Page-by-Page Design Specifications

### PAGE 1: Parent Portal Landing Page

**Layout Structure:**
```
Header (Fixed, 80px height):
├── Left: School Logo (40px height) + "SOAP - Smart Admission Portal"
├── Right: [LOGIN] button (if not logged in) or User menu

Hero Section (400px height):
├── Background: Light gradient (#f8fafc to white)
├── Center: Welcome message + School name (H1)
├── KCPE Index input card (centered, 400px width)

Features Section (300px height):
├── 4 columns with icons and benefits
├── Icons: 📱 💾 📊 📲 (or similar)

Footer (100px height):
├── Contact information
├── Help links
```

**Detailed Elements:**

**KCPE Input Card:**
- White background, subtle shadow
- Padding: 32px
- Border radius: 12px
- Title: "Enter Your Child's KCPE Index Number" (H2)
- Input field: Full width, large (48px height)
- Verify button: Primary blue, full width
- Helper text: "Example: 12345678901" (gray, small)

**Feature Icons:**
- Size: 48px x 48px
- Style: Outline icons in Education Blue
- Text below: Inter Medium, 16px
- Spacing: 24px between icon and text

### PAGE 2: Application Form

**Layout Structure:**
```
Header: Same as landing page
Progress Bar: Full width, below header
Form Container: Centered, 800px max width
├── Section 1: Student Information
├── Section 2: Parent/Guardian Information  
├── Section 3: Emergency Contact
├── Section 4: Additional Information
Footer Buttons: Save Draft + Continue
```

**Form Sections:**
- Each section in white card with 24px padding
- Section title: H3, Education Blue
- 16px spacing between form fields
- Required fields marked with red asterisk
- Validation messages below fields in red

**Progress Indicator:**
- Show "Step 2 of 4" text
- Visual progress bar (50% filled)
- Current step highlighted in blue

### PAGE 3: Document Upload

**Layout Structure:**
```
Header: Same as previous pages
Progress Bar: 75% complete
Document List: 3 required documents
├── Each document in separate card
├── Upload area or file preview
├── Status indicator (uploaded/pending/missing)
Action Buttons: Back + Submit Application
```

**Document Cards:**
- White background, border radius 8px
- Document title: H3, dark gray
- Status badge: Top right corner
- Upload area: Dashed border, drag-and-drop
- File preview: File icon + name + size + [View] [Replace] buttons

**Upload States:**
- Empty: Dashed border, upload icon, "Drag & drop or browse"
- Uploading: Progress bar, "Uploading..." text
- Uploaded: File icon, name, size, action buttons
- Error: Red border, error message

### PAGE 4: Application Status Dashboard

**Layout Structure:**
```
Header: Same as previous pages
Status Summary Card: Top of page
Document Status Cards: Grid layout (3 columns)
Next Steps Card: Bottom section
Action Buttons: Edit Application + Download Receipt
```

**Status Summary Card:**
- Large card, full width
- Student name and details (left side)
- Large status indicator (right side)
- Submission timestamp
- Background color matches status (subtle)

**Document Status Grid:**
- 3 cards in row (desktop), stacked (mobile)
- Each card shows document type, status, admin feedback
- Action buttons for rejected documents

### PAGE 5: Admin Dashboard

**Layout Structure:**
```
Header: Logo + Navigation tabs + User menu
Stats Cards: 4 cards showing key metrics
Applications Table: Sortable, filterable data table
Pagination: Bottom of table
```

**Navigation Tabs:**
- Dashboard, Applications, Students, Analytics, Settings
- Active tab: Blue background, white text
- Inactive tabs: Gray text, hover effect

**Stats Cards:**
- 4 cards in row: Total, Approved, Pending, Rejected
- Large number (H1), description below
- Icon on right side
- Color coding for each metric

**Data Table:**
- Striped rows for readability
- Sortable columns (arrow indicators)
- Action buttons in last column
- Search box above table
- Filter dropdowns

### PAGE 6: Analytics Dashboard (Admin)

**Layout Structure:**
```
Header: Same as admin pages with Analytics tab active
Time Period Selector: Last 7 days, 30 days, This year dropdown
Key Metrics Row: 4 large stat cards
Charts Section: 2x2 grid of interactive charts
Data Tables: Recent activity and detailed breakdowns
Export Options: PDF/Excel download buttons
```

**Key Metrics Cards (Top Row):**
```
Card 1 - Total Applications:
├── Large number: "1,247" (H1, 48px)
├── Label: "Total Applications" (16px, gray)
├── Trend: "+23 today" (14px, green with ↗️ arrow)
├── Icon: 📊 (top right, 32px, blue)
├── Background: White card with subtle shadow

Card 2 - Approval Rate:
├── Large number: "89.2%" (H1, 48px, green)
├── Label: "Approval Rate" (16px, gray)
├── Trend: "+2.1% vs last month" (14px, green)
├── Icon: ✅ (top right, 32px, green)

Card 3 - Average Processing Time:
├── Large number: "2.3" (H1, 48px, orange)
├── Label: "Days Average Processing" (16px, gray)
├── Trend: "-0.5 days improved" (14px, green)
├── Icon: ⏱️ (top right, 32px, orange)

Card 4 - Document Issues:
├── Large number: "12%" (H1, 48px, red)
├── Label: "Documents Requiring Resubmission" (16px, gray)
├── Trend: "-3% vs last month" (14px, green)
├── Icon: 📄 (top right, 32px, red)
```

**Charts Section (2x2 Grid):**

**Chart 1 - Applications Over Time (Line Chart):**
```
Position: Top left
Size: 600px x 300px
Type: Line chart with smooth curves
Data: Daily application submissions over selected period
Colors: Blue line (#1e40af), light blue fill
X-axis: Dates
Y-axis: Number of applications
Hover: Show exact date and count
Grid: Subtle gray lines
```

**Chart 2 - Application Status Breakdown (Donut Chart):**
```
Position: Top right
Size: 400px x 300px
Type: Donut chart with center text
Data: Approved (green), Pending (orange), Rejected (red)
Center Text: "1,247 Total"
Legend: Right side with percentages
Colors: Match status color system
Hover: Show count and percentage
```

**Chart 3 - Document Upload Success Rate (Bar Chart):**
```
Position: Bottom left
Size: 600px x 300px
Type: Horizontal bar chart
Data: Success rate for each document type
Bars: KCPE Slip (95%), Birth Cert (87%), Medical Form (78%)
Colors: Green gradient for success rates
Labels: Document names on Y-axis, percentages on bars
Grid: Vertical lines for easy reading
```

**Chart 4 - Daily Processing Volume (Area Chart):**
```
Position: Bottom right
Size: 600px x 300px
Type: Stacked area chart
Data: Applications processed per day (approved vs rejected)
Colors: Green area (approved), red area (rejected)
X-axis: Dates
Y-axis: Number processed
Legend: Top right
Fill: Semi-transparent areas
```

**Interactive Features:**
- **Time Period Selector:** Dropdown to change all charts simultaneously
- **Chart Hover:** Detailed tooltips with exact values
- **Chart Click:** Drill down to detailed data
- **Export Buttons:** "Export PDF Report" and "Export Excel Data"
- **Real-time Updates:** Charts refresh every 5 minutes

### PAGE 7: Application Review (Admin)

**Layout Structure:**
```
Header: Same as admin pages
Breadcrumb: Dashboard > Applications > Review
Student Info Card: Top section
Documents Grid: 3 columns showing each document
Decision Panel: Bottom section with approval options
```

**Document Viewer:**
- Click to open modal with full document view
- Zoom controls, download option
- Approve/Reject buttons below each document
- Feedback text area for rejections

**Decision Panel:**
- Radio buttons for decision options
- Comments text area
- Action buttons: Save Decision + Send SMS

### PAGE 8: Detailed Reports (Admin)

**Layout Structure:**
```
Header: Same as admin pages
Report Filters: Date range, status, document type filters
Summary Cards: Key insights based on filters
Data Visualization: Large charts and graphs
Export Section: Multiple format options
Data Table: Detailed records with sorting/filtering
```

**Advanced Charts:**

**Student Demographics Chart (Pie Chart):**
```
Type: Multi-level pie chart
Inner Ring: Boarding vs Day students
Outer Ring: County breakdown
Colors: Blue gradient palette
Size: 500px x 500px
Interactive: Click to filter other charts
```

**Processing Time Trends (Multi-line Chart):**
```
Type: Multi-line chart
Lines: Average, Median, 90th percentile processing times
Colors: Blue, green, orange lines
Time Period: Configurable (weekly, monthly, yearly)
Y-axis: Days
Annotations: Mark policy changes or system updates
```

**Document Rejection Reasons (Horizontal Bar Chart):**
```
Type: Horizontal stacked bar chart
Categories: Each document type
Segments: Different rejection reasons
Colors: Red gradient palette
Labels: Reason text with percentages
Sorting: By frequency (highest first)
```

**Geographic Distribution (Map Visualization):**
```
Type: Kenya county map with color coding
Data: Application density by county
Colors: Light to dark blue gradient
Hover: County name and application count
Legend: Color scale with ranges
Interactive: Click county to filter data
```

---

## 4. Mobile Responsive Specifications

### Breakpoints
```
Mobile: 320px - 767px
Tablet: 768px - 1023px  
Desktop: 1024px+
```

### Mobile Adaptations
```
Navigation: Hamburger menu
Cards: Full width, reduced padding
Tables: Horizontal scroll or card layout
Forms: Single column, larger touch targets
Buttons: Minimum 44px height for touch
Text: Slightly smaller but still readable
```

---

## 5. Accessibility Requirements

### WCAG 2.1 AA Compliance
```
Color Contrast: Minimum 4.5:1 for normal text
Focus Indicators: Visible outline on all interactive elements
Keyboard Navigation: All functions accessible via keyboard
Screen Readers: Proper ARIA labels and semantic HTML
Alt Text: All images have descriptive alt text
Form Labels: All inputs properly labeled
Error Messages: Clear, descriptive error messages
```

### Inclusive Design
```
Font Size: Minimum 14px on mobile
Touch Targets: Minimum 44px x 44px
Loading States: Clear indicators for all async actions
Error Recovery: Clear paths to fix errors
Language: Simple, clear English (some Swahili for key terms)
```

---

## 6. Animation & Interaction Guidelines

### Micro-interactions
```
Button Hover: Subtle color change (0.2s transition)
Form Focus: Smooth border color change
Loading States: Spinner or skeleton screens
Success Actions: Brief green checkmark animation
Page Transitions: Subtle fade (0.3s)
```

### Performance
```
Image Optimization: WebP format, lazy loading
Animation: CSS transforms only (no layout changes)
Loading: Progressive enhancement, graceful degradation
Caching: Aggressive caching for static assets
```

---

## 7. Implementation Notes for Developer

### CSS Framework
```
Base: Bootstrap 5 for grid and utilities
Custom CSS: Override Bootstrap variables
Icons: Font Awesome or Heroicons
Fonts: Google Fonts (Inter + JetBrains Mono)
```

### Chart Library Specifications

**Recommended Chart Library: Chart.js or ApexCharts**
```
Chart.js Configuration:
- Responsive: true
- Maintain aspect ratio: false
- Animation duration: 750ms
- Font family: 'Inter'
- Grid color: #f3f4f6
- Tooltip background: #1f2937
- Tooltip text color: white
```

**Chart Styling Standards:**
```
Line Charts:
- Line width: 3px
- Point radius: 4px (hover: 6px)
- Fill opacity: 0.1
- Smooth curves: tension 0.4

Bar Charts:
- Border radius: 4px (top corners)
- Bar thickness: 20px
- Spacing: 8px between bars
- Hover: Increase opacity to 0.8

Donut Charts:
- Cutout: 60% (for center text)
- Border width: 0
- Hover: Increase segment by 10px

Color Gradients:
- Blue: Linear gradient from #3b82f6 to #1e40af
- Green: Linear gradient from #10b981 to #059669
- Orange: Linear gradient from #f59e0b to #d97706
- Red: Linear gradient from #ef4444 to #dc2626
```

**Interactive Features:**
```
Hover Effects:
- Smooth transitions (0.3s)
- Tooltip with exact values
- Highlight related data points
- Cursor pointer on clickable elements

Click Actions:
- Drill down to detailed view
- Filter other charts
- Open modal with more data
- Export specific data segment

Loading States:
- Skeleton screens while loading
- Smooth fade-in when data loads
- Loading spinner for real-time updates
```

### Component Structure
```
Razor Components: Reusable UI components
Partial Views: Shared layout elements
CSS Classes: BEM methodology for custom styles
JavaScript: Chart.js for data visualization
Real-time: SignalR for live chart updates
```

### File Organization
```
/wwwroot/css/
├── bootstrap.min.css
├── site.css (global styles)
├── parent.css (parent portal specific)
├── admin.css (admin dashboard specific)
└── analytics.css (charts and dashboard specific)

/wwwroot/js/
├── site.js (global functionality)
├── parent.js (parent portal features)
├── admin.js (admin dashboard features)
├── charts.js (chart configurations and interactions)
└── analytics.js (analytics dashboard functionality)

/wwwroot/lib/
├── chart.js/ (or apexcharts/)
├── datatables/
└── export-libs/ (for PDF/Excel export)
```

**Analytics-Specific CSS Classes:**
```
.analytics-card {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  border: 1px solid #f3f4f6;
}

.metric-card {
  text-align: center;
  position: relative;
  min-height: 120px;
}

.metric-number {
  font-size: 48px;
  font-weight: 700;
  line-height: 1;
  margin-bottom: 8px;
}

.metric-label {
  font-size: 16px;
  color: #64748b;
  margin-bottom: 4px;
}

.metric-trend {
  font-size: 14px;
  font-weight: 500;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
}

.metric-trend.positive {
  color: #059669;
}

.metric-trend.negative {
  color: #dc2626;
}

.chart-container {
  position: relative;
  height: 300px;
  margin: 16px 0;
}

.chart-title {
  font-size: 18px;
  font-weight: 600;
  margin-bottom: 16px;
  color: #1e293b;
}

.export-buttons {
  display: flex;
  gap: 12px;
  margin-top: 24px;
}

.time-selector {
  margin-bottom: 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
```

---

## 8. Design Deliverables Checklist

For each page, provide:
- [ ] Desktop mockup (1200px width)
- [ ] Tablet mockup (768px width)  
- [ ] Mobile mockup (375px width)
- [ ] Interactive states (hover, focus, active)
- [ ] Error states and validation messages
- [ ] Loading states and empty states
- [ ] Component specifications
- [ ] Asset exports (icons, images)

---

## 9. Brand Customization Guide

### School Branding
```
Logo Placement: Header left, max height 40px
Color Customization: Primary blue can be changed per school
Typography: School name in custom font if provided
Favicon: School logo adapted for 32x32px
```

### White Label Options
```
Remove "SOAP" branding: Replace with school name only
Custom domain: school-name.admission-portal.co.ke
Custom colors: Match school's brand colors
Custom footer: School contact information
```

This specification provides everything needed to create professional, accessible, and user-friendly designs for the SOAP platform. Each page is designed to be intuitive for Kenyan parents and school administrators while maintaining modern web standards.