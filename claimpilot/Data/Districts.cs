using System.Collections.ObjectModel;
using ClaimPilot.Models;

namespace ClaimPilot.Data;

/// <summary>
/// All 94 U.S. federal judicial districts. No courts are invented.
///
/// IMPORTANT CORRECTION vs. the original task checklist: New York has FOUR
/// districts (Eastern, Northern, Southern, Western) — not three — and Ohio has
/// TWO districts (Northern, Southern) — not three. The corrected grouping is:
/// 4-district states CA, NY, TX (12); 3-district states AL, FL, GA, IL, LA, NC,
/// OK, PA, TN (27); 2-district states AR, IN, IA, KY, MI, MS, MO, OH, VA, WA, WV,
/// WI (24); 1-district states (26); plus the District of Columbia, the District
/// of Puerto Rico, the District of Guam, the District of the Virgin Islands,
/// and the District of the Northern Mariana Islands (5). 12+27+24+26+5 = 94.
///
/// Website URLs follow the official https://www.&lt;abbrev&gt;.uscourts.gov pattern.
/// Spot-verified via web search on 2026-10-10: gud (Guam), nmid (N. Mariana
/// Islands), vid (Virgin Islands), prd (Puerto Rico), dcd (District of
/// Columbia) — all confirmed live on their uscourts.gov subdomains.
/// </summary>
public static class Districts
{
    public static IReadOnlyList<DistrictInfo> All { get; } = new List<DistrictInfo>
    {
        // ---- Alabama (3) ----
        new DistrictInfo("Middle District of Alabama", "Alabama", "Montgomery, Dothan, Opelika", "https://www.almd.uscourts.gov"),
        new DistrictInfo("Northern District of Alabama", "Alabama", "Birmingham, Huntsville, Tuscaloosa, Anniston, Decatur", "https://www.alnd.uscourts.gov"),
        new DistrictInfo("Southern District of Alabama", "Alabama", "Mobile, Selma", "https://www.alsd.uscourts.gov"),
        // ---- Alaska (1) ----
        new DistrictInfo("District of Alaska", "Alaska", "Anchorage, Fairbanks, Juneau, Ketchikan, Nome", "https://www.akd.uscourts.gov"),
        // ---- Arizona (1) ----
        new DistrictInfo("District of Arizona", "Arizona", "Phoenix, Tucson, Yuma, Flagstaff, Prescott", "https://www.azd.uscourts.gov"),
        // ---- Arkansas (2) ----
        new DistrictInfo("Eastern District of Arkansas", "Arkansas", "Little Rock, Helena, Jonesboro, Pine Bluff, Batesville", "https://www.ared.uscourts.gov"),
        new DistrictInfo("Western District of Arkansas", "Arkansas", "Fort Smith, Fayetteville, El Dorado, Texarkana, Hot Springs", "https://www.arwd.uscourts.gov"),
        // ---- California (4) ----
        new DistrictInfo("Central District of California", "California", "Los Angeles, Santa Ana, Riverside", "https://www.cacd.uscourts.gov"),
        new DistrictInfo("Eastern District of California", "California", "Sacramento, Fresno, Bakersfield, Redding", "https://www.caed.uscourts.gov"),
        new DistrictInfo("Northern District of California", "California", "San Francisco, Oakland, San Jose, Eureka", "https://www.cand.uscourts.gov"),
        new DistrictInfo("Southern District of California", "California", "San Diego, El Centro", "https://www.casd.uscourts.gov"),
        // ---- Colorado (1) ----
        new DistrictInfo("District of Colorado", "Colorado", "Denver, Colorado Springs, Grand Junction, Durango", "https://www.cod.uscourts.gov"),
        // ---- Connecticut (1) ----
        new DistrictInfo("District of Connecticut", "Connecticut", "New Haven, Hartford, Bridgeport", "https://www.ctd.uscourts.gov"),
        // ---- Delaware (1) ----
        new DistrictInfo("District of Delaware", "Delaware", "Wilmington", "https://www.ded.uscourts.gov"),
        // ---- District of Columbia (1) ----
        new DistrictInfo("District of Columbia", "District of Columbia", "Washington", "https://www.dcd.uscourts.gov"),
        // ---- Florida (3) ----
        new DistrictInfo("Middle District of Florida", "Florida", "Tampa, Orlando, Jacksonville, Fort Myers, Ocala", "https://www.flmd.uscourts.gov"),
        new DistrictInfo("Northern District of Florida", "Florida", "Tallahassee, Pensacola, Gainesville, Panama City", "https://www.flnd.uscourts.gov"),
        new DistrictInfo("Southern District of Florida", "Florida", "Miami, Fort Lauderdale, West Palm Beach, Key West, Fort Pierce", "https://www.flsd.uscourts.gov"),
        // ---- Georgia (3) ----
        new DistrictInfo("Middle District of Georgia", "Georgia", "Macon, Columbus, Albany, Valdosta, Athens", "https://www.gamd.uscourts.gov"),
        new DistrictInfo("Northern District of Georgia", "Georgia", "Atlanta, Rome, Newnan, Gainesville", "https://www.gand.uscourts.gov"),
        new DistrictInfo("Southern District of Georgia", "Georgia", "Savannah, Augusta, Brunswick, Waycross, Dublin", "https://www.gasd.uscourts.gov"),
        // ---- Hawaii (1) ----
        new DistrictInfo("District of Hawaii", "Hawaii", "Honolulu", "https://www.hid.uscourts.gov"),
        // ---- Idaho (1) ----
        new DistrictInfo("District of Idaho", "Idaho", "Boise, Pocatello, Coeur d'Alene", "https://www.idd.uscourts.gov"),
        // ---- Illinois (3) ----
        new DistrictInfo("Central District of Illinois", "Illinois", "Springfield, Urbana, Peoria, Rock Island", "https://www.ilcd.uscourts.gov"),
        new DistrictInfo("Northern District of Illinois", "Illinois", "Chicago, Rockford", "https://www.ilnd.uscourts.gov"),
        new DistrictInfo("Southern District of Illinois", "Illinois", "East St. Louis, Benton", "https://www.ilsd.uscourts.gov"),
        // ---- Indiana (2) ----
        new DistrictInfo("Northern District of Indiana", "Indiana", "South Bend, Hammond, Fort Wayne, Lafayette", "https://www.innd.uscourts.gov"),
        new DistrictInfo("Southern District of Indiana", "Indiana", "Indianapolis, Evansville, Terre Haute, New Albany", "https://www.insd.uscourts.gov"),
        // ---- Iowa (2) ----
        new DistrictInfo("Northern District of Iowa", "Iowa", "Cedar Rapids, Sioux City, Waterloo, Dubuque", "https://www.iand.uscourts.gov"),
        new DistrictInfo("Southern District of Iowa", "Iowa", "Des Moines, Davenport, Council Bluffs", "https://www.iasd.uscourts.gov"),
        // ---- Kansas (1) ----
        new DistrictInfo("District of Kansas", "Kansas", "Kansas City, Wichita, Topeka", "https://www.ksd.uscourts.gov"),
        // ---- Kentucky (2) ----
        new DistrictInfo("Eastern District of Kentucky", "Kentucky", "Lexington, Frankfort, Covington, Pikeville, London", "https://www.kyed.uscourts.gov"),
        new DistrictInfo("Western District of Kentucky", "Kentucky", "Louisville, Bowling Green, Owensboro, Paducah", "https://www.kywd.uscourts.gov"),
        // ---- Louisiana (3) ----
        new DistrictInfo("Eastern District of Louisiana", "Louisiana", "New Orleans", "https://www.laed.uscourts.gov"),
        new DistrictInfo("Middle District of Louisiana", "Louisiana", "Baton Rouge", "https://www.lamd.uscourts.gov"),
        new DistrictInfo("Western District of Louisiana", "Louisiana", "Shreveport, Lafayette, Lake Charles, Alexandria, Monroe", "https://www.lawd.uscourts.gov"),
        // ---- Maine (1) ----
        new DistrictInfo("District of Maine", "Maine", "Portland, Bangor", "https://www.med.uscourts.gov"),
        // ---- Maryland (1) ----
        new DistrictInfo("District of Maryland", "Maryland", "Baltimore, Greenbelt", "https://www.mdd.uscourts.gov"),
        // ---- Massachusetts (1) ----
        new DistrictInfo("District of Massachusetts", "Massachusetts", "Boston, Worcester, Springfield", "https://www.mad.uscourts.gov"),
        // ---- Michigan (2) ----
        new DistrictInfo("Eastern District of Michigan", "Michigan", "Detroit, Flint, Bay City, Port Huron, Ann Arbor", "https://www.mied.uscourts.gov"),
        new DistrictInfo("Western District of Michigan", "Michigan", "Grand Rapids, Kalamazoo, Lansing, Marquette, Traverse City", "https://www.miwd.uscourts.gov"),
        // ---- Minnesota (1) ----
        new DistrictInfo("District of Minnesota", "Minnesota", "Minneapolis, St. Paul, Duluth, Fergus Falls", "https://www.mnd.uscourts.gov"),
        // ---- Mississippi (2) ----
        new DistrictInfo("Northern District of Mississippi", "Mississippi", "Oxford, Aberdeen, Greenville, Clarksdale", "https://www.msnd.uscourts.gov"),
        new DistrictInfo("Southern District of Mississippi", "Mississippi", "Jackson, Gulfport, Hattiesburg, Natchez", "https://www.mssd.uscourts.gov"),
        // ---- Missouri (2) ----
        new DistrictInfo("Eastern District of Missouri", "Missouri", "St. Louis, Hannibal, Cape Girardeau", "https://www.moed.uscourts.gov"),
        new DistrictInfo("Western District of Missouri", "Missouri", "Kansas City, Springfield, St. Joseph, Jefferson City", "https://www.mowd.uscourts.gov"),
        // ---- Montana (1) ----
        new DistrictInfo("District of Montana", "Montana", "Billings, Missoula, Helena, Great Falls, Butte", "https://www.mtd.uscourts.gov"),
        // ---- Nebraska (1) ----
        new DistrictInfo("District of Nebraska", "Nebraska", "Omaha, Lincoln, North Platte", "https://www.ned.uscourts.gov"),
        // ---- Nevada (1) ----
        new DistrictInfo("District of Nevada", "Nevada", "Las Vegas, Reno, Elko", "https://www.nvd.uscourts.gov"),
        // ---- New Hampshire (1) ----
        new DistrictInfo("District of New Hampshire", "New Hampshire", "Concord", "https://www.nhd.uscourts.gov"),
        // ---- New Jersey (1) ----
        new DistrictInfo("District of New Jersey", "New Jersey", "Newark, Trenton, Camden", "https://www.njd.uscourts.gov"),
        // ---- New Mexico (1) ----
        new DistrictInfo("District of New Mexico", "New Mexico", "Albuquerque, Las Cruces, Santa Fe", "https://www.nmd.uscourts.gov"),
        // ---- New York (4) ----
        new DistrictInfo("Eastern District of New York", "New York", "Brooklyn, Central Islip", "https://www.nyed.uscourts.gov"),
        new DistrictInfo("Northern District of New York", "New York", "Syracuse, Albany, Utica, Binghamton, Plattsburgh", "https://www.nynd.uscourts.gov"),
        new DistrictInfo("Southern District of New York", "New York", "Manhattan, White Plains", "https://www.nysd.uscourts.gov"),
        new DistrictInfo("Western District of New York", "New York", "Buffalo, Rochester", "https://www.nywd.uscourts.gov"),
        // ---- North Carolina (3) ----
        new DistrictInfo("Eastern District of North Carolina", "North Carolina", "Raleigh, Greenville, New Bern, Wilmington, Fayetteville", "https://www.nced.uscourts.gov"),
        new DistrictInfo("Middle District of North Carolina", "North Carolina", "Greensboro, Winston-Salem, Durham", "https://www.ncmd.uscourts.gov"),
        new DistrictInfo("Western District of North Carolina", "North Carolina", "Charlotte, Asheville, Statesville", "https://www.ncwd.uscourts.gov"),
        // ---- North Dakota (1) ----
        new DistrictInfo("District of North Dakota", "North Dakota", "Fargo, Bismarck", "https://www.ndd.uscourts.gov"),
        // ---- Ohio (2) ----
        new DistrictInfo("Northern District of Ohio", "Ohio", "Cleveland, Toledo, Akron, Youngstown", "https://www.ohnd.uscourts.gov"),
        new DistrictInfo("Southern District of Ohio", "Ohio", "Cincinnati, Columbus, Dayton", "https://www.ohsd.uscourts.gov"),
        // ---- Oklahoma (3) ----
        new DistrictInfo("Eastern District of Oklahoma", "Oklahoma", "Muskogee, Durant, Hugo, Poteau, Ardmore", "https://www.oked.uscourts.gov"),
        new DistrictInfo("Northern District of Oklahoma", "Oklahoma", "Tulsa, Bartlesville, Miami, Pawhuska", "https://www.oknd.uscourts.gov"),
        new DistrictInfo("Western District of Oklahoma", "Oklahoma", "Oklahoma City, Enid, Lawton, Woodward, Guthrie", "https://www.okwd.uscourts.gov"),
        // ---- Oregon (1) ----
        new DistrictInfo("District of Oregon", "Oregon", "Portland, Eugene, Medford, Pendleton", "https://www.ord.uscourts.gov"),
        // ---- Pennsylvania (3) ----
        new DistrictInfo("Eastern District of Pennsylvania", "Pennsylvania", "Philadelphia, Reading, Allentown, Easton", "https://www.paed.uscourts.gov"),
        new DistrictInfo("Middle District of Pennsylvania", "Pennsylvania", "Harrisburg, Scranton, Williamsport, Wilkes-Barre", "https://www.pamd.uscourts.gov"),
        new DistrictInfo("Western District of Pennsylvania", "Pennsylvania", "Pittsburgh, Erie, Johnstown", "https://www.pawd.uscourts.gov"),
        // ---- Rhode Island (1) ----
        new DistrictInfo("District of Rhode Island", "Rhode Island", "Providence", "https://www.rid.uscourts.gov"),
        // ---- South Carolina (1) ----
        new DistrictInfo("District of South Carolina", "South Carolina", "Columbia, Charleston, Greenville, Florence", "https://www.scd.uscourts.gov"),
        // ---- South Dakota (1) ----
        new DistrictInfo("District of South Dakota", "South Dakota", "Sioux Falls, Rapid City, Pierre, Aberdeen", "https://www.sdd.uscourts.gov"),
        // ---- Tennessee (3) ----
        new DistrictInfo("Eastern District of Tennessee", "Tennessee", "Knoxville, Chattanooga, Greeneville, Winchester", "https://www.tned.uscourts.gov"),
        new DistrictInfo("Middle District of Tennessee", "Tennessee", "Nashville, Columbia, Cookeville", "https://www.tnmd.uscourts.gov"),
        new DistrictInfo("Western District of Tennessee", "Tennessee", "Memphis, Jackson", "https://www.tnwd.uscourts.gov"),
        // ---- Texas (4) ----
        new DistrictInfo("Eastern District of Texas", "Texas", "Tyler, Beaumont, Sherman, Plano, Texarkana", "https://www.txed.uscourts.gov"),
        new DistrictInfo("Northern District of Texas", "Texas", "Dallas, Fort Worth, Amarillo, Lubbock, Abilene, Wichita Falls", "https://www.txnd.uscourts.gov"),
        new DistrictInfo("Southern District of Texas", "Texas", "Houston, Galveston, Corpus Christi, Brownsville, Laredo, McAllen", "https://www.txs.uscourts.gov"),
        new DistrictInfo("Western District of Texas", "Texas", "San Antonio, Austin, El Paso, Waco, Midland, Del Rio", "https://www.txwd.uscourts.gov"),
        // ---- Utah (1) ----
        new DistrictInfo("District of Utah", "Utah", "Salt Lake City, St. George", "https://www.utd.uscourts.gov"),
        // ---- Vermont (1) ----
        new DistrictInfo("District of Vermont", "Vermont", "Burlington, Rutland, Brattleboro", "https://www.vtd.uscourts.gov"),
        // ---- Virginia (2) ----
        new DistrictInfo("Eastern District of Virginia", "Virginia", "Alexandria, Richmond, Norfolk, Newport News", "https://www.vaed.uscourts.gov"),
        new DistrictInfo("Western District of Virginia", "Virginia", "Roanoke, Charlottesville, Lynchburg, Harrisonburg, Abingdon", "https://www.vawd.uscourts.gov"),
        // ---- Washington (2) ----
        new DistrictInfo("Eastern District of Washington", "Washington", "Spokane, Yakima, Richland", "https://www.waed.uscourts.gov"),
        new DistrictInfo("Western District of Washington", "Washington", "Seattle, Tacoma", "https://www.wawd.uscourts.gov"),
        // ---- West Virginia (2) ----
        new DistrictInfo("Northern District of West Virginia", "West Virginia", "Wheeling, Clarksburg, Martinsburg, Elkins", "https://www.wvnd.uscourts.gov"),
        new DistrictInfo("Southern District of West Virginia", "West Virginia", "Charleston, Huntington, Beckley, Bluefield, Parkersburg", "https://www.wvsd.uscourts.gov"),
        // ---- Wisconsin (2) ----
        new DistrictInfo("Eastern District of Wisconsin", "Wisconsin", "Milwaukee, Green Bay, Oshkosh", "https://www.wied.uscourts.gov"),
        new DistrictInfo("Western District of Wisconsin", "Wisconsin", "Madison, Eau Claire, La Crosse, Wausau", "https://www.wiwd.uscourts.gov"),
        // ---- Wyoming (1) ----
        new DistrictInfo("District of Wyoming", "Wyoming", "Cheyenne, Casper, Lander", "https://www.wyd.uscourts.gov"),
        // ---- U.S. territories with federal district courts (4) ----
        new DistrictInfo("District of Puerto Rico", "Puerto Rico", "San Juan, Ponce, Mayagüez", "https://www.prd.uscourts.gov"),
        new DistrictInfo("District of Guam", "Guam", "Hagåtña", "https://www.gud.uscourts.gov"),
        new DistrictInfo("District of the Virgin Islands", "U.S. Virgin Islands", "St. Thomas, St. Croix", "https://www.vid.uscourts.gov"),
        new DistrictInfo("District of the Northern Mariana Islands", "Northern Mariana Islands", "Saipan", "https://www.nmid.uscourts.gov"),
    };

    /// <summary>Must be 94. Verified at startup by the Court Finder view model.</summary>
    public static int Count => All.Count;

    /// <summary>
    /// Special entries that are NOT federal district courts, shown alongside
    /// search results with an explanatory note. American Samoa has no federal
    /// district court — nothing is invented here.
    /// </summary>
    public static IReadOnlyList<DistrictInfo> SpecialEntries { get; } = new List<DistrictInfo>
    {
        new DistrictInfo("American Samoa — no federal district court", "American Samoa", "Pago Pago", "https://www.uscourts.gov/court-locator")
        {
            IsSpecialEntry = true,
            Note = "American Samoa has NO federal district court. Its court of general jurisdiction is the High Court of American Samoa. " +
                   "Do not assume a local federal courthouse exists — verify where a federal matter arising in American Samoa must be filed, " +
                   "and consider talking to an attorney."
        }
    };

    /// <summary>All districts plus special entries, for the Court Finder search.</summary>
    public static IReadOnlyList<DistrictInfo> Searchable => All.Concat(SpecialEntries).ToList();
}
