MissionVerify
===============================================================================
Author: Michael Gaisser (mjgaisser@gmail.com)
Version 2.2.1
Date: 2023.12.23
===============================================================================

This utility performs simple quality verification steps on X-wing series
mission files.

===================
VERSION HISTORY

v2.2.1 - 23 Dec 2033
 - Fixed XvT/XWA Dep And/Or was checking Arr 12AO34
 - Fixed XvT/XWA Global Goal triggers checking 1/2 twice instead of 1/2 and 3/4

v2.2 - 06 Jul 2022
 - XWA order check now scans all orders instead of just O1 in SP1's region
 - Limit messages now dynamically use the actual limits

v2.1 - 15 Mar 2021
 - Added "OR true" and "AND false" trigger detection for 1AO2 and 3OR4 triggers [YOGEME/#48]
 
v2.0.1 - 29 Nov 2020
 - Rebuild with newer version of Idmr.Platform

v2.0 - 15 Feb 2018
 - Code rewrite
 - Implemented Idmr.Platform
 - Removed Containers, backdrops, etc from AI and Orders checks [YOGEME/#15]
 - AI messages changed "no AI" to "basic AI", downgraded to "Note" [#3]
 - Fixed crash when too many messages generated [#2]
 - Multiple player FlightGroups downgraded to "Warning"

===================
USAGE

The program is primarily intended to be run automatically while saving a
mission in YOGEME. It can also be run manually, with a drag-and-drop region
to drop custom missions from an explorer window.

The following checks and categorizations are currently implemented. More
rigorous checks may be included at a later time.

-=ALL PLATFORMS=-
-Note
 * No in-flight Messages
 * No "Mission Completed" message
 * Basic AI setting

-Warning
 * Multiple player FlightGroups
 * Mission Craft limit exceeded, does not guarantee concurrent existance
 * FlightGroup missing orders
 * Default Briefing duration
 * No Briefing events
 * No Mission description/pre-flight questions.
 * Trigger logical errors
 
-Failure
 * Mission FlightGroup limit exceeded
 * Mission Message limit exceeded
 * No player FlightGroup

-=TIE=-
-Note
 * No post-mission questions
 
-Failure
 * Global Goals logical error
 
-=XWA=-
-Failure
 * Player FlightGroup missing mothership

===============================================================================
Copyright © 2022 Michael Gaisser
This program and related files are licensed under the Mozilla Public License.
See License.txt for the full text. If for some reason the license was not
distributed with this program, you can obtain the full text of the license at
http://mozilla.org/MPL/2.0/.

"Star Wars" and related items are trademarks of LucasFilm Ltd and
LucasArts Entertainment Co.

This software is provided "as is" without warranty of any kind; including that
the software is free of defects, merchantable, fit for a particular purpose or
non-infringing. See the full license text for more details.