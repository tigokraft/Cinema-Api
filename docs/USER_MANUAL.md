# Cinema Booking System - User Manual

Welcome to the Cinema Booking System! This guide will walk you through all the features available to users and administrators.

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [Browsing Movies](#browsing-movies)
3. [Booking Tickets](#booking-tickets)
4. [Managing Your Account](#managing-your-account)
5. [Admin Guide](#admin-guide)

---

## Getting Started

### Creating an Account

1. **Click "Sign Up"** in the navigation bar
2. **Enter your email address** - This will be used for ticket confirmations
3. **Create a password** - Must be at least 6 characters
4. **Confirm your password**
5. **Click "Create Account"**

### Email Verification

After registration, you'll receive a **6-digit verification code** via email:

1. Check your email inbox (and spam folder)
2. Enter the code in the verification page
3. Click "Verify Email"
4. You can now log in to your account

> **Note:** The verification code expires in 15 minutes. Click "Resend Code" if needed.

### Logging In

1. Click **"Sign In"** in the navigation bar
2. Enter your **email or username**
3. Enter your **password**
4. Click **"Sign In"**

---

## Browsing Movies

### Homepage

The homepage displays:
- **Featured Movie** - A highlighted movie with trailer option
- **Now Showing** - Currently available movies
- **Upcoming Screenings** - Quick access to showtimes

> **Tip:** You can browse movies without logging in!

### Movie Details

Click on any movie to see:
- **Movie poster and description**
- **Runtime and genre**
- **Available showtimes** grouped by date
- **Theater locations**

### Filtering Showtimes

On the movie detail page:
1. **Select a date** from the dropdown to filter by day
2. **Select a theater** to see screenings at a specific location
3. Click **"Clear Filters"** to see all showtimes again

---

## Booking Tickets

### Step 1: Select Seats

1. Click on a showtime to open the seat selection
2. The seat map shows:
   - 🟩 **Available seats** - Click to select
   - 🟦 **Your selection** - Click again to deselect
   - 🟥 **Occupied seats** - Already taken

3. **Select multiple seats** by clicking each one
4. The **total price** updates automatically
5. Click **"Continue to Booking"**

> **Note:** If you're not logged in, you'll be redirected to login. After logging in, you'll return to the booking page with your seats still selected.

### Step 2: Your Information

1. Review the **booking summary** showing:
   - Movie title and showtime
   - Selected seats
   - Total price

2. Verify your **contact information**:
   - First name
   - Last name
   - Email address (ticket will be sent here)

3. Click **"Continue to Payment"**

### Step 3: Payment

1. Review your order summary
2. **Apply a promo code** (optional):
   - Enter the code
   - Click "Apply"
   - The discount will be reflected in the total

3. Enter payment details:
   - Card number
   - Cardholder name
   - Expiry date
   - CVV

4. Click **"Complete Purchase"**

> **Note:** This is a demo system - no actual payment is processed!

### Step 4: Confirmation

After successful purchase:
- Your **e-ticket(s)** are displayed
- A **confirmation email** is sent
- Tickets are saved to your account

---

## Managing Your Account

### Profile

Access your profile from the navigation menu:
- View your account information
- Update your name and email
- See your booking history

### My Tickets

View all your purchased tickets:

| Status | Description |
|--------|-------------|
| **Active** | Valid ticket for upcoming show |
| **Used** | Ticket has been checked in |
| **Cancelled** | Refunded ticket |
| **Expired** | Show has passed |

### Cancelling a Ticket

1. Go to **My Tickets**
2. Find the ticket you want to cancel
3. Click **"Cancel Ticket"**
4. Confirm the cancellation

> **Note:** Cancellations may be restricted close to showtime.

### Logging Out

Click your username in the navigation, then click **"Logout"**

---

## Admin Guide

### Accessing Admin Dashboard

Administrators can access the dashboard via **/Admin**

> **Requirement:** Your account must have the "Admin" role.

### Managing Movies

**Adding a Movie:**
1. Go to **Movies** in the admin menu
2. Click **"Add Movie"**
3. Fill in the details:
   - Title
   - Description
   - Genre
   - Duration (minutes)
   - Poster URL
   - Trailer URL (optional)
4. Click **"Save"**

**Editing a Movie:**
1. Find the movie in the list
2. Click **"Edit"**
3. Update the information
4. Click **"Save Changes"**

**Setting Featured Movie:**
1. Edit the movie
2. Check **"Featured"**
3. Save changes

### Managing Theaters

**Adding a Theater:**
1. Go to **Theaters**
2. Click **"Add Theater"**
3. Enter:
   - Theater name
   - Address
   - Number of rows
   - Seats per row
4. Click **"Save"**

### Managing Screenings

**Creating a Screening:**
1. Go to **Screenings**
2. Click **"Add Screening"**
3. Select:
   - Movie
   - Theater
   - Room
   - Date and time
   - Ticket price
4. Click **"Save"**

**Bulk Scheduling:**
Use the schedule generator to create multiple screenings:
1. Select movie and theater
2. Choose date range
3. Set times
4. Click **"Generate Schedule"**

### Managing Promo Codes

**Creating a Promo Code:**
1. Go to **Promo Codes**
2. Click **"Add Promo Code"**
3. Enter:
   - Code (e.g., SAVE20)
   - Discount type (percentage or fixed)
   - Discount value
   - Expiry date
   - Usage limit
4. Click **"Save"**

### Managing Users

View all registered users:
- See registration date
- Check verification status
- Modify user roles

---

## Troubleshooting

### "Invalid credentials" on login
- Verify your email is correct
- Ensure you've verified your email address
- Try resetting your password

### Didn't receive verification email
- Check your spam/junk folder
- Click "Resend Code" on the verification page
- Ensure your email is correct

### Seat appears occupied but isn't
- Refresh the page to get the latest seat availability
- Another user may have just purchased it

### Payment failed
- This is a demo system - payments are simulated
- Try again or contact support

---

## Support

For technical issues or questions:
- Check the homepage for announcements
- Contact your system administrator

---

*Last updated: January 2026*
