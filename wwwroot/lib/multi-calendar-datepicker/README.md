# Multi-Calendar DatePicker

A comprehensive jQuery plugin that supports **Gregorian**, **Persian (Jalaali)**, and **Hijri (Islamic)** calendar systems with advanced features including time picker, RTL support, and extensive customization options.

## 🌐 Live Demo

**Try it out:** [https://delphiassistant.github.io/MultiCalendarDatePicker/](https://delphiassistant.github.io/MultiCalendarDatePicker/)

Experience all calendar systems, themes, and features in action with interactive examples.

## 🌟 Features

### 📅 Calendar Systems
- **Gregorian Calendar**: Standard Western calendar with leap year calculations (1900-2100)
- **Persian Calendar**: Solar calendar with verified leap year algorithm (1200-1500)
- **Hijri Calendar**: Lunar Islamic calendar with improved leap year detection (1000-2000)

### 🎨 User Interface
- **Responsive Design**: Works on desktop and mobile devices
- **RTL Support**: Full right-to-left support for Persian and Arabic
- **Themes**: Light and dark themes
- **Customizable**: Extensive styling options via CSS variables

### ⏰ Time Picker
- **24-hour Format**: HH:mm, HH:mm:ss
- **12-hour Format**: hh:mm A, hh:mm:ss A
- **Configurable**: Show/hide seconds, custom stepping
- **Localized**: Hour, minute, second labels in multiple languages

### 🌍 Internationalization
- **Languages**: English, Persian (Farsi), Arabic
- **Auto-detection**: Automatic RTL detection for Persian/Hijri calendars
- **Custom Locales**: Easy to add new languages

### 🔧 Advanced Features
- **Date Constraints**: Min/max date, disabled dates, disabled weekdays
- **Keyboard Navigation**: Full keyboard support with arrow keys
- **Data Attributes**: Initialize via HTML data attributes
- **API Methods**: Programmatic control and manipulation
- **Event System**: Rich event handling for all interactions
- **Input Validation**: Real-time validation with auto-correction for manual input
- **Visual Feedback**: Animated feedback for corrected and invalid inputs
- **Editable Year Input**: Year field in datepicker title is always visible and editable
- **Real-Time Year Validation**: Year validation happens during typing, not just on blur
- **Bidirectional Sync**: Changes in datepicker year input automatically update attached textbox
- **Year Range Validation**: Automatic correction of invalid years to nearest valid value
- **Configurable Timeouts**: Customizable auto-correction timeout for mobile-friendly typing
- **Mobile Optimization**: Number input spinners visible on mobile devices
- **Custom RTL Input**: Better control over RTL input styling with `.mcdp-rtl-input` class

## 📦 Installation

### Bundled Library

The project includes a **bundled Moment.js library** (`moment-bundled.js`) that combines:
- **Moment.js Core** (2.29.4)
- **Persian/Jalaali Calendar** with verified leap year detection (1300-2100)
- **Hijri/Islamic Calendar** with verified leap year algorithms (1300-1600)

This single file replaces the need for separate Moment.js, `moment-jalaali`, and `moment-hijri` libraries, making setup simpler and more reliable.

#### Key Improvements in Bundled Library:
- ✅ **Accurate Persian conversion** using verified leap year data
- ✅ **Reliable Hijri conversion** with improved algorithms
- ✅ **Browser-compatible** - Works in all modern browsers
- ✅ **Optimized performance** - Single file reduces HTTP requests
- ✅ **Verified accuracy** - Tested against known date pairs

### Dependencies
```html
<!-- Required Libraries -->
<script src="lib/jquery-3.7.1.min.js"></script>
<script src="lib/bootstrap-5.3.0.min.js"></script>
<script src="lib/moment-bundled.js"></script>

<!-- DatePicker Files -->
<link href="lib/multi-calendar-datepicker.css" rel="stylesheet">
<script src="lib/multi-calendar-datepicker.js"></script>
```

### File Organization
```
lib/
├── multi-calendar-datepicker.js    # Main plugin file
├── multi-calendar-datepicker.css   # Styles and themes
├── moment-bundled.js              # Combined Moment.js with calendar extensions
├── jquery-3.7.1.min.js            # jQuery library
└── bootstrap-5.3.0.min.js          # Bootstrap JavaScript

moment.js-fixes/                   # Archived files
├── moment-2.29.4.min.js          # Original Moment.js
├── moment-jalali.js              # Original Persian library
├── moment-hijri.js                # Original Hijri library
└── CALENDAR_FIXES.md             # Documentation of fixes
```

### Basic Usage
```html
<input type="text" id="datepicker" placeholder="YYYY/MM/DD">
```

```javascript
$('#datepicker').multiCalendarDatePicker({
    calendar: 'gregorian',
    locale: 'en',
    format: 'YYYY/MM/DD'
});
```

## 🚀 Quick Start

### Gregorian Calendar
```javascript
$('#gregorian-date').multiCalendarDatePicker({
    calendar: 'gregorian',
    locale: 'en',
    format: 'YYYY/MM/DD',
    timePicker: true,
    timeFormat: 'HH:mm'
});
```

### Persian Calendar
```javascript
$('#persian-date').multiCalendarDatePicker({
    calendar: 'persian',
    locale: 'fa',
    format: 'YYYY/MM/DD',
    rtl: true
});
```

### Hijri Calendar
```javascript
$('#hijri-date').multiCalendarDatePicker({
    calendar: 'hijri',
    locale: 'ar',
    format: 'YYYY/MM/DD',
    rtl: true
});
```

## ⚙️ Configuration Options

### Core Settings
| Option | Type | Default | Possible Values | Effect |
|--------|------|---------|-----------------|--------|
| `calendar` | string | `'gregorian'` | `'gregorian'`, `'persian'`, `'hijri'` | Determines calendar system and year ranges |
| `locale` | string | `'en'` | `'en'`, `'fa'`, `'ar'` | Sets language for month names, weekdays, and UI text |
| `format` | string | `'YYYY/MM/DD'` | `'YYYY/MM/DD'`, `'MM/DD/YYYY'`, `'DD/MM/YYYY'` | Controls date display and input parsing format |
| `rtl` | boolean | `null` | `true`, `false`, `null` | Enables RTL layout (auto-detected for Persian/Hijri) |
| `theme` | string | `'light'` | `'light'`, `'dark'` | Sets color scheme and visual appearance |

### Time Picker Settings
| Option | Type | Default | Possible Values | Effect |
|--------|------|---------|-----------------|--------|
| `timePicker` | boolean | `false` | `true`, `false` | Enables/disables time selection interface |
| `timeFormat` | string | `'HH:mm'` | `'HH:mm'`, `'HH:mm:ss'`, `'hh:mm A'`, `'hh:mm:ss A'` | Controls time display format |
| `showSeconds` | boolean | `false` | `true`, `false` | Shows/hides seconds input field |
| `use24Hour` | boolean | `true` | `true`, `false` | Uses 24-hour vs 12-hour time format |
| `stepping` | number | `1` | `1`, `5`, `15`, `30` | Sets minute increment step for time picker |

### UI Behavior
| Option | Type | Default | Possible Values | Effect |
|--------|------|---------|-----------------|--------|
| `placement` | string | `'bottom-start'` | `'bottom-start'`, `'bottom-end'`, `'top-start'`, `'top-end'` | Controls picker position relative to input |
| `showToday` | boolean | `true` | `true`, `false` | Shows/hides "Today" button |
| `showClear` | boolean | `true` | `true`, `false` | Shows/hides "Clear" button |
| `showClose` | boolean | `true` | `true`, `false` | Shows/hides "Close" button |
| `autoClose` | boolean | `true` | `true`, `false` | Auto-closes picker after date selection |
| `hideAfterSelect` | boolean | `true` | `true`, `false` | Hides picker immediately after selection |
| `keyboardNavigation` | boolean | `true` | `true`, `false` | Enables arrow key navigation |
| `todayHighlight` | boolean | `true` | `true`, `false` | Highlights current date in calendar |
| `allowInput` | boolean | `true` | `true`, `false` | Allows manual typing in input field |

### Date Constraints
| Option | Type | Default | Possible Values | Effect |
|--------|------|---------|-----------------|--------|
| `minDate` | string | `null` | Date string in format, `null` | Prevents selection of dates before this date |
| `maxDate` | string | `null` | Date string in format, `null` | Prevents selection of dates after this date |
| `disabledDates` | array | `[]` | Array of date strings | Disables specific dates from selection |
| `disabledDays` | array | `[]` | Array of numbers (0-6) | Disables specific weekdays (0=Sunday) |

### Auto-Correction Settings
| Option | Type | Default | Possible Values | Effect |
|--------|------|---------|-----------------|--------|
| `autoCorrectionTimeout` | number | `3000` | Any positive number (ms) | Sets timeout for auto-correction of invalid years and dates |

## 🎯 Configuration Examples

### Basic Usage with Default Timeout
```javascript
$('#datepicker').multiCalendarDatePicker({
    calendar: 'gregorian',
    locale: 'en',
    format: 'YYYY/MM/DD'
    // autoCorrectionTimeout defaults to 3000ms
});
```

### Mobile-Optimized Timeout
```javascript
$('#mobile-datepicker').multiCalendarDatePicker({
    calendar: 'persian',
    locale: 'fa',
    format: 'YYYY/MM/DD',
    rtl: true,
    autoCorrectionTimeout: 4000 // 4 seconds for slower mobile typing
});
```

### Fast Desktop Timeout
```javascript
$('#desktop-datepicker').multiCalendarDatePicker({
    calendar: 'hijri',
    locale: 'ar',
    format: 'YYYY/MM/DD',
    rtl: true,
    autoCorrectionTimeout: 1000 // 1 second for quick desktop typing
});
```

### Accessibility-Friendly Timeout
```javascript
$('#accessible-datepicker').multiCalendarDatePicker({
    calendar: 'gregorian',
    locale: 'en',
    format: 'YYYY/MM/DD',
    autoCorrectionTimeout: 5000 // 5 seconds for users who type slowly
});
```

### Complete Configuration Example
```javascript
$('#advanced-datepicker').multiCalendarDatePicker({
    // Core Settings
    calendar: 'persian',
    locale: 'fa',
    format: 'YYYY/MM/DD',
    rtl: true,
    theme: 'dark',
    
    // Time Picker Settings
    timePicker: true,
    timeFormat: 'HH:mm:ss',
    showSeconds: true,
    use24Hour: true,
    stepping: 5,
    
    // UI Behavior
    placement: 'bottom-end',
    showToday: true,
    showClear: true,
    showClose: true,
    autoClose: false,
    hideAfterSelect: false,
    keyboardNavigation: true,
    todayHighlight: true,
    allowInput: true,
    
    // Date Constraints
    minDate: '1200/01/01',
    maxDate: '1500/12/29',
    disabledDates: ['1200/01/01', '1200/12/29'],
    disabledDays: [0, 6], // Disable weekends
    
    // Auto-Correction Settings
    autoCorrectionTimeout: 3500 // Custom timeout for this instance
});
```

## 📝 Data Attributes

Initialize datepickers using HTML data attributes:

```html
<input type="text" 
       data-mcdp
       data-calendar="hijri"
       data-locale="ar"
       data-theme="dark"
       data-time-picker="true"
       data-time-format="hh:mm A">
```

### Available Data Attributes
- `data-calendar` - Calendar system (`gregorian`, `persian`, `hijri`)
- `data-locale` - Language (`en`, `fa`, `ar`)
- `data-format` - Date format (`YYYY/MM/DD`, `MM/DD/YYYY`, `DD/MM/YYYY`)
- `data-rtl` - RTL mode (`true`, `false`)
- `data-theme` - Theme (`light`, `dark`)
- `data-time-picker` - Enable time picker (`true`, `false`)
- `data-time-format` - Time format (`HH:mm`, `HH:mm:ss`, `hh:mm A`, `hh:mm:ss A`)
- `data-show-seconds` - Show seconds (`true`, `false`)
- `data-use24-hour` - 24-hour format (`true`, `false`)
- `data-stepping` - Minute stepping (`1`, `5`, `15`, `30`)
- `data-placement` - Picker placement (`bottom-start`, `bottom-end`, `top-start`, `top-end`)
- `data-show-today` - Show today button (`true`, `false`)
- `data-show-clear` - Show clear button (`true`, `false`)
- `data-show-close` - Show close button (`true`, `false`)
- `data-auto-close` - Auto-close after selection (`true`, `false`)
- `data-hide-after-select` - Hide after date selection (`true`, `false`)
- `data-keyboard-navigation` - Enable keyboard navigation (`true`, `false`)
- `data-today-highlight` - Highlight today's date (`true`, `false`)
- `data-allow-input` - Allow manual input (`true`, `false`)
- `data-min-date` - Minimum date (date string)
- `data-max-date` - Maximum date (date string)
- `data-disabled-dates` - Disabled dates (comma-separated)
- `data-disabled-days` - Disabled weekdays (comma-separated, 0-6)
- `data-auto-correction-timeout` - Auto-correction timeout in milliseconds

## 🔧 API Methods

### Basic Methods
```javascript
// Show/hide picker
$('#datepicker').multiCalendarDatePicker('show');
$('#datepicker').multiCalendarDatePicker('hide');
$('#datepicker').multiCalendarDatePicker('toggle');

// Set/get date
$('#datepicker').multiCalendarDatePicker('setDate', '2024/12/25');
var date = $('#datepicker').multiCalendarDatePicker('getDate');

// Clear date
$('#datepicker').multiCalendarDatePicker('clear');

// Set today
$('#datepicker').multiCalendarDatePicker('today');

// Get formatted date
var formatted = $('#datepicker').multiCalendarDatePicker('getFormattedDate');
```

### Calendar-Specific API Usage

#### Method 1: Separate Input Elements
```javascript
// Initialize different calendars
$('#gregorian-input').multiCalendarDatePicker({
    calendar: 'gregorian',
    locale: 'en',
    format: 'YYYY/MM/DD'
});

$('#persian-input').multiCalendarDatePicker({
    calendar: 'persian',
    locale: 'fa',
    format: 'YYYY/MM/DD',
    rtl: true
});

$('#hijri-input').multiCalendarDatePicker({
    calendar: 'hijri',
    locale: 'ar',
    format: 'YYYY/MM/DD',
    rtl: true
});

// Use API methods on specific calendars
$('#gregorian-input').multiCalendarDatePicker('today');    // Sets Gregorian today
$('#persian-input').multiCalendarDatePicker('today');      // Sets Persian today
$('#hijri-input').multiCalendarDatePicker('today');        // Sets Hijri today
```

#### Method 2: Dynamic Calendar Switching
```javascript
// Initialize with one calendar
$('#universal-input').multiCalendarDatePicker({
    calendar: 'gregorian',
    locale: 'en',
    format: 'YYYY/MM/DD'
});

// Switch to different calendars dynamically
function switchToPersian() {
    $('#universal-input').multiCalendarDatePicker('setOptions', {
        calendar: 'persian',
        locale: 'fa',
        rtl: true
    });
}

function switchToHijri() {
    $('#universal-input').multiCalendarDatePicker('setOptions', {
        calendar: 'hijri',
        locale: 'ar',
        rtl: true
    });
}

// Now API methods work with the current calendar
$('#universal-input').multiCalendarDatePicker('today');  // Uses current calendar
```

### Configuration Methods
```javascript
// Update options
$('#datepicker').multiCalendarDatePicker('setOptions', {
    theme: 'dark',
    timePicker: true
});

// Get current options
var options = $('#datepicker').multiCalendarDatePicker('getOptions');

// Destroy instance
$('#datepicker').multiCalendarDatePicker('destroy');
```

## 📅 Calendar Systems Details

### Gregorian Calendar
- **Range**: 1900-2100
- **Week Start**: Sunday
- **Leap Year Rule**: Divisible by 4, except century years unless divisible by 400
- **Leap Day**: February 29th
- **Year Validation**: Auto-corrects invalid years to nearest valid value (min: 1900, max: 2100)

### Persian Calendar (Jalaali)
- **Range**: 1200-1500
- **Week Start**: Saturday
- **Leap Year Algorithm**: Verified list (1300-2100) with 33-year cycle fallback
- **New Year**: Nowruz (March 20-21)
- **Leap Day**: 30th day of Esfand (month 12)
- **Year Validation**: Auto-corrects invalid years to nearest valid value (min: 1200, max: 1500)
- **Editable Year**: Year input in title is always visible and editable

### Hijri Calendar (Islamic)
- **Range**: 1000-2000
- **Week Start**: Saturday
- **Leap Year Algorithm**: Verified list (1300-1600) with improved 30-year cycle fallback
- **Leap Years**: 11 leap years per 30-year cycle
- **Leap Day**: 30th day of Dhul-Hijjah (month 12)
- **Fixed**: Correctly shows 30 days for month 12 in leap years (e.g., 1401, 1406, 1411)
- **Year Validation**: Auto-corrects invalid years to nearest valid value (min: 1000, max: 2000)

## ⏱️ Auto-Correction Timeout Configuration

The `autoCorrectionTimeout` parameter controls how long the system waits before automatically correcting invalid years and dates. This is particularly useful for mobile devices where typing can be slower.

### Default Behavior
```javascript
// Default 3000ms timeout
$('#datepicker').multiCalendarDatePicker({
    calendar: 'gregorian',
    locale: 'en',
    format: 'YYYY/MM/DD'
    // autoCorrectionTimeout: 3000 (default)
});
```

### Use Cases

#### Mobile Applications (4000-5000ms)
```javascript
$('#mobile-datepicker').multiCalendarDatePicker({
    calendar: 'persian',
    locale: 'fa',
    format: 'YYYY/MM/DD',
    rtl: true,
    autoCorrectionTimeout: 4000 // Slower for touch typing
});
```

#### Desktop Applications (1000-2000ms)
```javascript
$('#desktop-datepicker').multiCalendarDatePicker({
    calendar: 'hijri',
    locale: 'ar',
    format: 'YYYY/MM/DD',
    rtl: true,
    autoCorrectionTimeout: 1500 // Faster for keyboard typing
});
```

#### Accessibility Applications (5000ms+)
```javascript
$('#accessible-datepicker').multiCalendarDatePicker({
    calendar: 'gregorian',
    locale: 'en',
    format: 'YYYY/MM/DD',
    autoCorrectionTimeout: 5000 // Extra time for users who type slowly
});
```

### How It Works

1. **Year Input Validation**: When typing in the datepicker's year input field
   - 4-digit invalid years: Corrected immediately
   - 3-digit invalid years: Corrected after timeout
   - Valid years: Updated immediately

2. **Attached Input Validation**: When typing in the main textbox
   - Invalid dates: Corrected after timeout
   - Valid dates: Processed immediately

3. **Timeout Behavior**: 
   - Timer resets on each keystroke
   - Only triggers when user stops typing
   - Prevents excessive corrections during typing

## 🎨 Styling & Theming

### CSS Variables
```css
:root {
    --mcdp-primary-color: #007bff;
    --mcdp-background: #ffffff;
    --mcdp-text-color: #333333;
    --mcdp-border-color: #dee2e6;
    --mcdp-hover-color: #f8f9fa;
    --mcdp-selected-color: #007bff;
    --mcdp-today-color: #28a745;
    --mcdp-disabled-color: #6c757d;
}
```

### Dark Theme
```css
.mcdp-theme-dark {
    --mcdp-background: #343a40;
    --mcdp-text-color: #ffffff;
    --mcdp-border-color: #495057;
    --mcdp-hover-color: #495057;
}
```

## ✅ Input Validation & Auto-Correction

### Real-Time Validation
The datepicker includes comprehensive input validation that works in real-time as users type:

```javascript
// Automatic validation examples
"2025/19/24" → "2025/12/24" (month corrected)
"2025/02/30" → "2025/02/28" (day corrected for non-leap year)
"1401/12/30" → Valid (leap year in Hijri calendar)
"1402/12/30" → "1402/12/29" (corrected for non-leap year)
"25:70:90" → "25:59:59" (time values corrected)

// Year validation examples
"1104" in Persian calendar → "1200" (corrected to minimum valid year)
"1600" in Persian calendar → "1500" (corrected to maximum valid year)
"2025" in Gregorian calendar → Valid (within 1900-2100 range)
```

### Validation Rules
- **Month**: 1-12 (auto-corrected if out of range)
- **Day**: 1 to maximum days in month (considering leap years)
- **Year**: Calendar-specific ranges (Gregorian: 1900-2100, Persian: 1200-1500, Hijri: 1000-2000)
- **Hour**: 0-23 (24-hour) or 1-12 (12-hour format)
- **Minute/Second**: 0-59 (auto-corrected if out of range)

### Year Input Features
- **Always Visible**: Year input in datepicker title is always visible and editable (not just on hover)
- **Real-Time Validation**: Year validation happens during typing, not just on blur
- **Auto-Correction**: Invalid years are automatically corrected to nearest valid value
- **Bidirectional Sync**: Changes in datepicker year input automatically update the attached textbox
- **Visual Feedback**: Red border for invalid years, normal styling for valid years

### Visual Feedback
- **🟡 Yellow Pulse**: Input was corrected automatically
- **🔴 Red Shake**: Input is invalid and couldn't be corrected
- **🔴 Red Border**: Year input shows red border for invalid years
- **✅ Green**: Input is valid

### Performance Optimized
- **300ms debounce** prevents excessive validation during typing
- **Efficient algorithms** for leap year calculations
- **Minimal DOM updates** for smooth user experience
- **Smart validation**: Only validates complete years (4+ digits) during typing

## 🎯 Year Input Features

### Always Visible Year Input
The year field in the datepicker title is now always visible and editable, providing immediate access to year modification:

```javascript
// Year input is always visible in the datepicker title
// Users can click and edit the year directly
$('#datepicker').multiCalendarDatePicker({
    calendar: 'persian',
    locale: 'fa',
    format: 'YYYY/MM/DD'
});
```

### Real-Time Year Validation
Year validation happens during typing, providing immediate feedback:

```javascript
// Examples of real-time year validation
// Persian Calendar (range: 1200-1500)
"1104" → Auto-corrects to "1200" (minimum valid year)
"1600" → Auto-corrects to "1500" (maximum valid year)
"1403" → Valid (within range)

// Gregorian Calendar (range: 1900-2100)
"1800" → Auto-corrects to "1900" (minimum valid year)
"2200" → Auto-corrects to "2100" (maximum valid year)
"2024" → Valid (within range)

// Hijri Calendar (range: 1000-2000)
"900" → Auto-corrects to "1000" (minimum valid year)
"2100" → Auto-corrects to "2000" (maximum valid year)
"1445" → Valid (within range)
```

### Bidirectional Synchronization
Changes in the datepicker year input automatically update the attached textbox:

```javascript
// When user changes year in datepicker title
// 1. Year input updates (e.g., 1403 → 1404)
// 2. Calendar display updates
// 3. Attached textbox updates (e.g., "1403/01/15" → "1404/01/15")
// 4. mcdp:change event fires with new date
```

### Visual Feedback
- **Normal State**: Year input has visible border and background
- **Invalid Year**: Red border appears for out-of-range years
- **Valid Year**: Normal styling for years within valid range
- **Auto-Correction**: Invalid years are automatically corrected to nearest valid value

### Smart Validation Timing
- **During Typing**: Only validates complete years (4+ digits) to avoid premature corrections
- **On Blur**: Full validation and correction for any remaining invalid input
- **Performance**: Optimized to prevent excessive validation during typing

## 📱 Events

### Available Events
```javascript
$('#datepicker').on('mcdp:init', function(e, data) {
    console.log('DatePicker initialized', data.instance);
});

$('#datepicker').on('mcdp:show', function(e) {
    console.log('DatePicker shown');
});

$('#datepicker').on('mcdp:hide', function(e) {
    console.log('DatePicker hidden');
});

$('#datepicker').on('mcdp:select', function(e, data) {
    console.log('Date selected', data.date, data.formatted);
});

$('#datepicker').on('mcdp:change', function(e, data) {
    console.log('Date changed', data.date, data.formatted);
});

$('#datepicker').on('mcdp:clear', function(e) {
    console.log('Date cleared');
});

$('#datepicker').on('mcdp:today', function(e, data) {
    console.log('Today set', data.date);
});

$('#datepicker').on('mcdp:timeChange', function(e, data) {
    console.log('Time changed', data.date, data.formatted);
});

$('#datepicker').on('mcdp:error', function(e, data) {
    console.log('Error occurred', data.error);
});
```

## 🔍 Browser Support

- **Chrome**: 60+
- **Firefox**: 55+
- **Safari**: 12+
- **Edge**: 79+
- **Mobile Browsers**: iOS Safari, Chrome Mobile
- **IE**: 11+ (with graceful degradation)

## 🐛 Troubleshooting

### Common Issues

1. **Datepicker not showing**
   - Check if jQuery is loaded
   - Verify CSS file is included
   - Check browser console for errors

2. **RTL not working**
   - Ensure `rtl: true` is set for Persian/Hijri calendars
   - Check if RTL CSS is properly loaded

3. **Date parsing errors**
   - Verify date format matches input
   - Check if date is within supported range
   - Ensure calendar type is correct

4. **Leap year issues**
   - Clear browser cache (Ctrl+Shift+R)
   - Verify you're using the latest version
   - Check if date is in supported range

5. **Manual input validation**
   - Invalid dates are automatically corrected
   - Visual feedback shows corrections (yellow pulse) or errors (red shake)
   - Real-time validation with 300ms debounce for performance

### Debug Mode
```javascript
// Enable debug logging
$('#datepicker').multiCalendarDatePicker({
    debug: true,
    // ... other options
});
```

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📞 Support

For issues, questions, or contributions:
- Create an issue on GitHub
- Check the documentation
- Review the examples in `index.html`

## 🔄 Changelog

### Latest Updates (v2.3)

#### 🎯 Major New Features
- ✅ **Configurable Auto-Correction Timeout** - New `autoCorrectionTimeout` parameter for customizable validation timing
- ✅ **Mobile Optimization** - Number input spinners now visible on mobile devices
- ✅ **Custom RTL Input Class** - New `.mcdp-rtl-input` class for better RTL input control
- ✅ **Consistent Timeout Behavior** - Both year input and attached input use same configurable timeout

#### 🎯 Previous Major Features (v2.2)
- ✅ **Editable Year Input** - Year field in datepicker title is now always visible and editable (not just on hover)
- ✅ **Real-Time Year Validation** - Year validation happens during typing, not just on blur
- ✅ **Bidirectional Sync** - Changes in datepicker year input automatically update the attached textbox
- ✅ **Year Range Validation** - Automatic correction of invalid years to nearest valid value
- ✅ **Month Name Updates** - Month name in title now updates correctly after year changes and month navigation

#### 🎯 Previous Major Fixes (v2.1)
- ✅ **Fixed Hijri leap year detection** - Now correctly shows 30 days for month 12 in leap years (1401, 1406, 1411, etc.)
- ✅ **Disabled incorrect moment function** - Forces use of verified leap year arrays for accuracy
- ✅ **Enhanced input validation** - Real-time validation with auto-correction for manual date/time input
- ✅ **Visual feedback system** - Yellow pulse for corrections, red shake for invalid inputs

#### Persian Calendar Improvements
- ✅ **Verified leap year algorithm** (1200-1500)
- ✅ **Fixed leap year discrepancies** (1393, 1403, etc.)
- ✅ **Accurate leap year detection** using verified data
- ✅ **Corrected "today" calculation** - Fixed Persian date display

#### Hijri Calendar Improvements
- ✅ **Extended year range** from 1-1500 to 1000-2000
- ✅ **Improved leap year algorithm** with better cycle calculation
- ✅ **Fixed date parsing** for years after 1500
- ✅ **Enhanced fallback algorithm** for years outside verified range
- ✅ **Fixed month 12 leap year display** - Now correctly shows 30 days in leap years

#### General Improvements
- ✅ **Enhanced data attributes support** for all configuration options
- ✅ **Better RTL support** with improved layout and custom `.mcdp-rtl-input` class
- ✅ **Improved time picker integration** with localized labels
- ✅ **Fixed event handling** for better user experience
- ✅ **Enhanced date validation** with extended ranges
- ✅ **Manual input validation** - Validates month (1-12), day (1-max for month), hour (0-23/1-12), minute/second (0-59)
- ✅ **Configurable validation timing** - Customizable timeout for auto-correction (default 3000ms)
- ✅ **Textbox synchronization** - Picker UI always reflects textbox value
- ✅ **Year input styling** - Always visible year input with proper borders and hover effects
- ✅ **Smart validation timing** - Only validates complete years (4+ digits) during typing for better UX
- ✅ **Mobile-friendly spinners** - Number input spinners visible on all devices
- ✅ **Consistent timeout behavior** - Unified timeout system for all auto-corrections

#### Bug Fixes
- ✅ **Fixed year parsing** (1591 showing as 159)
- ✅ **Fixed date reflection** for typed dates after 1500
- ✅ **Fixed leap year detection** for all calendar types
- ✅ **Fixed time formatting** and parsing issues
- ✅ **Fixed RTL arrow icons** with proper flipping
- ✅ **Fixed calendar key errors** - Corrected uppercase/lowercase calendar constants
- ✅ **Fixed programmatic show** - Resolved "Show Picker" button conflicts
- ✅ **Fixed month name updates** - Month name now updates correctly after year changes and month navigation
- ✅ **Fixed year input visibility** - Year input is now always visible instead of only on hover
- ✅ **Fixed year validation timing** - Year validation now happens during typing, not just on blur
- ✅ **Fixed textbox synchronization** - Year changes in datepicker now properly update the attached textbox

---

**Multi-Calendar DatePicker** - Supporting multiple calendar systems with accuracy and precision! 🌍📅