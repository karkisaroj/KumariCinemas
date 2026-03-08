-- ================================================================
-- KUMARI CINEMAS - VERIFICATION QUERIES
-- Use these to verify all new features are working correctly
-- ================================================================

-- 1. CHECK TICKET STATUSES
-- ================================================================
-- This shows all possible ticket statuses in your system
SELECT TICKET_STATUS, COUNT(*) AS COUNT
FROM TICKET
GROUP BY TICKET_STATUS
ORDER BY TICKET_STATUS;

-- Expected Results:
-- Booked         (Orange - Reserved but not paid)
-- Purchased      (Green - Paid and confirmed)
-- Cancelled      (Red - Manually cancelled)
-- Auto-Cancelled (Purple - System cancelled due to no payment)


-- 2. FIND UNPAID TICKETS (Candidates for Auto-Cancellation)
-- ================================================================
SELECT t.TICKET_ID,
       t.BOOKING_TIME,
       t.PURCHASE_DATE,
       t.TICKET_STATUS,
       s.SHOW_DATE,
       s.SHOW_TIME,
       ROUND((s.SHOW_DATE + s.SHOW_TIME - SYSDATE) * 24, 2) AS HOURS_UNTIL_SHOW
FROM TICKET t
JOIN "TktShowHallMovCust" tshmc ON t.TICKET_ID = tshmc.TICKET_ID
JOIN SHOWTIME s ON tshmc.SHOW_ID = s.SHOW_ID
WHERE t.TICKET_STATUS = 'Booked'
  AND t.PURCHASE_DATE IS NULL
ORDER BY HOURS_UNTIL_SHOW;

-- If HOURS_UNTIL_SHOW < 1, ticket will be auto-cancelled on next page load


-- 3. CHECK MOVIE RELEASE DATES (For Dynamic Pricing)
-- ================================================================
SELECT MOVIE_ID,
       MOVIE_TITLE,
       MOVIE_RELEASE_DATE,
       ROUND(SYSDATE - MOVIE_RELEASE_DATE) AS DAYS_SINCE_RELEASE,
       CASE 
           WHEN (SYSDATE - MOVIE_RELEASE_DATE) <= 7 
           THEN 'YES - 30% Price Increase'
           ELSE 'NO - Normal Price'
       END AS NEW_RELEASE_PRICING
FROM MOVIE
WHERE MOVIE_RELEASE_DATE IS NOT NULL
ORDER BY MOVIE_RELEASE_DATE DESC;


-- 4. CHECK WEEKEND SHOWS (For Dynamic Pricing)
-- ================================================================
SELECT s.SHOW_ID,
       s.SHOW_DATE,
       TO_CHAR(s.SHOW_DATE, 'Day') AS DAY_OF_WEEK,
       CASE 
           WHEN TO_CHAR(s.SHOW_DATE, 'DY') IN ('FRI', 'SAT')
           THEN 'YES - 20% Price Increase'
           ELSE 'NO - Normal Price'
       END AS WEEKEND_PRICING
FROM SHOWTIME s
ORDER BY s.SHOW_DATE;


-- 5. VIEW COMPLETE TICKET DETAILS WITH PRICING INFO
-- ================================================================
SELECT t.TICKET_ID,
       c.CUSTOMER_NAME,
       m.MOVIE_TITLE,
       TO_CHAR(m.MOVIE_RELEASE_DATE, 'DD-MON-YYYY') AS RELEASE_DATE,
       ROUND(s.SHOW_DATE - m.MOVIE_RELEASE_DATE) AS DAYS_SINCE_RELEASE,
       s.SHOW_DATE,
       TO_CHAR(s.SHOW_DATE, 'Day') AS DAY_NAME,
       t.TICKET_PRICE,
       t.TICKET_STATUS,
       t.BOOKING_TIME,
       t.PURCHASE_DATE,
       CASE 
           WHEN t.PURCHASE_DATE IS NULL THEN 'NOT PAID'
           ELSE 'PAID'
       END AS PAYMENT_STATUS
FROM TICKET t
LEFT JOIN "TktShowHallMovCust" tshmc ON t.TICKET_ID = tshmc.TICKET_ID
LEFT JOIN CUSTOMER c ON tshmc.CUSTOMER_ID = c.CUSTOMER_ID
LEFT JOIN MOVIE m ON tshmc.MOVIE_ID = m.MOVIE_ID
LEFT JOIN SHOWTIME s ON tshmc.SHOW_ID = s.SHOW_ID
ORDER BY t.TICKET_ID DESC;


-- 6. SIMULATE AUTO-CANCELLATION (MANUAL TEST)
-- ================================================================
-- This query shows tickets that WOULD be auto-cancelled
SELECT t.TICKET_ID,
       t.TICKET_STATUS,
       s.SHOW_DATE,
       s.SHOW_TIME,
       (s.SHOW_DATE + s.SHOW_TIME - SYSDATE) * 24 AS HOURS_UNTIL_SHOW
FROM TICKET t
JOIN "TktShowHallMovCust" tshmc ON t.TICKET_ID = tshmc.TICKET_ID
JOIN SHOWTIME s ON tshmc.SHOW_ID = s.SHOW_ID
WHERE t.TICKET_STATUS = 'Booked'
  AND t.PURCHASE_DATE IS NULL
  AND (s.SHOW_DATE + s.SHOW_TIME - SYSDATE) * 24 < 1;

-- To manually trigger auto-cancellation for testing:
-- (Run this to cancel a specific ticket)
/*
UPDATE TICKET 
SET TICKET_STATUS = 'Auto-Cancelled'
WHERE TICKET_ID = <your_ticket_id>;
*/


-- 7. TEST DATA - CREATE TICKETS FOR DIFFERENT SCENARIOS
-- ================================================================
-- Run these to create test tickets (OPTIONAL)

-- Scenario 1: Normal booking (to be confirmed)
/*
INSERT INTO TICKET (TICKET_ID, BOOKING_TIME, PURCHASE_DATE, TICKET_STATUS, TICKET_PRICE, SEAT_NO)
VALUES ((SELECT NVL(MAX(TICKET_ID),0)+1 FROM TICKET), SYSDATE, NULL, 'Booked', 500, 10);
*/

-- Scenario 2: Confirmed purchase
/*
INSERT INTO TICKET (TICKET_ID, BOOKING_TIME, PURCHASE_DATE, TICKET_STATUS, TICKET_PRICE, SEAT_NO)
VALUES ((SELECT NVL(MAX(TICKET_ID),0)+1 FROM TICKET), SYSDATE, SYSDATE, 'Purchased', 650, 11);
*/

-- Scenario 3: Manually cancelled
/*
INSERT INTO TICKET (TICKET_ID, BOOKING_TIME, PURCHASE_DATE, TICKET_STATUS, TICKET_PRICE, SEAT_NO)
VALUES ((SELECT NVL(MAX(TICKET_ID),0)+1 FROM TICKET), SYSDATE, NULL, 'Cancelled', 500, 12);
*/


-- 8. PRICING CALCULATION TEST
-- ================================================================
-- Calculate what price SHOULD be for different scenarios

-- Base price: 500
-- Weekend (Fri/Sat): 500 * 1.2 = 600
-- New Release (< 7 days): 500 * 1.3 = 650
-- Both: MAX(1.2, 1.3) = 500 * 1.3 = 650

SELECT 
    500 AS BASE_PRICE,
    500 * 1.2 AS WEEKEND_PRICE,
    500 * 1.3 AS NEW_RELEASE_PRICE,
    500 * 1.3 AS BOTH_APPLIED
FROM DUAL;


-- 9. VERIFY PURCHASE_DATE IS NULL FOR NEW BOOKINGS
-- ================================================================
SELECT TICKET_ID,
       BOOKING_TIME,
       PURCHASE_DATE,
       TICKET_STATUS,
       CASE 
           WHEN PURCHASE_DATE IS NULL THEN 'Awaiting Payment'
           ELSE 'Payment Confirmed'
       END AS STATUS
FROM TICKET
ORDER BY TICKET_ID DESC;


-- 10. CHECK AUTO-CANCELLATION HISTORY
-- ================================================================
-- See which tickets were auto-cancelled
SELECT TICKET_ID,
       BOOKING_TIME,
       TICKET_STATUS,
       TICKET_PRICE
FROM TICKET
WHERE TICKET_STATUS = 'Auto-Cancelled'
ORDER BY BOOKING_TIME DESC;


-- ================================================================
-- SUMMARY STATISTICS
-- ================================================================
SELECT 
    COUNT(*) AS TOTAL_TICKETS,
    SUM(CASE WHEN TICKET_STATUS = 'Booked' THEN 1 ELSE 0 END) AS BOOKED_AWAITING_PAYMENT,
    SUM(CASE WHEN TICKET_STATUS = 'Purchased' THEN 1 ELSE 0 END) AS PURCHASED_PAID,
    SUM(CASE WHEN TICKET_STATUS = 'Cancelled' THEN 1 ELSE 0 END) AS MANUALLY_CANCELLED,
    SUM(CASE WHEN TICKET_STATUS = 'Auto-Cancelled' THEN 1 ELSE 0 END) AS AUTO_CANCELLED,
    SUM(CASE WHEN PURCHASE_DATE IS NULL THEN 1 ELSE 0 END) AS UNPAID_TICKETS,
    SUM(CASE WHEN PURCHASE_DATE IS NOT NULL THEN 1 ELSE 0 END) AS PAID_TICKETS,
    ROUND(AVG(TICKET_PRICE), 2) AS AVG_TICKET_PRICE
FROM TICKET;

-- ================================================================
-- END OF VERIFICATION QUERIES
-- ================================================================
