/*==============================================================================

Application:   Utility Function
Author:        John Gardner

Version:       V1.0
Date:          30th July 2005
Description:   Used to check the validity of an EU VAT number

Version:       V1.1
Date:          3rd August 2005
Description:   Lithunian legal entities & Maltese check digit checks added.

Version:       V1.2
Date:          20th October 2005
Description:   Italian checks refined (thanks Matteo Mike Peluso).

Version:       V1.3
Date:          16th November 2005
Description:   Error in GB numbers ending in 00 fixed (thanks Guy Dawson).

Version:       V1.4
Date:          28th September 2006
Description:   EU-type numbers added.

Version:       V1.5
Date:          1st January 2007
Description:   Romanian and Bulgarian numbers added.

Version:       V1.6
Date:          7th January 2007
Description:   Error with Slovenian numbers (thanks to Ales Hotko).

Version:       V1.7
Date:          10th February 2007
Description:   Romanian check digits added. Thanks to Dragu Costel for test suite.

Version:       V1.8
Date:          3rd August 2007
Description:   IE code modified to allow + and * in old format numbers. Thanks 
               to Antonin Moy of Spehere Solutions for pointing out the error.

Version:       V1.9
Date:          6th August 2007
Description:   BE code modified to make a specific check that the leading 
               character of 10 digit numbers is 0 (belts and braces).

Version:       V1.10
Date:          10th August 2007
Description:   Cypriot check digit support added.
               Check digit validation support for non-standard UK numbers

Version:       V1.11
Date:          25th September 2007
Description:   Spain check digit support for personal numbers.
               Author: David Perez Carmona

Version:       V1.12
Date:          23rd November 2009
Description:   GB code modified to take into account new style check digits.
               Thanks to Guy Dawson of Crossflight Ltd for pointing out the
               necessity.
  
Parameters:    toCheck - VAT number be checked. 

This function checks the value of the parameter for a valid European VAT number. 

If the number is found to be invalid format, the function returns a value of 
false. Otherwise it returns the VAT number re-formatted.
  
Example call:
  
  if (checkVATNumber (myVATNumber)) 
      alert ("VAT number has a valid format")
  else 
      alert ("VAT number has invalid format");
                    
------------------------------------------------------------------------------*/

function checkVATNumber (toCheck) {
 
  // Array holds the regular expressions for the valid VAT number
  var vatexp = new Array ();
  
  // To change the default country (e.g. from the UK to Germany - DE):
  //    1.  Change the country code in the defCCode variable below to "DE".
  //    2.  Remove the question mark from the regular expressions associated 
  //        with the UK VAT number: i.e. "(GB)?" -> "(GB)"
  //    3.  Add a question mark into the regular expression associated with
  //        Germany's number following the country code: i.e. "(DE)" -> "(DE)?"
  
  var defCCode = "GB";
  
  // Note - VAT codes without the "**" in the comment do not have check digit
  // checking.

  vatexp.push (/^(AT)U(\d{8})$/);                           //** Austria
  vatexp.push (/^(BE)(\d{9,10})$/);                         //** Belgium 
  vatexp.push (/^(BG)(\d{9,10})$/);                         // Bulgaria 
  vatexp.push (/^(CY)(\d{8}[A-Z])$/);                       //** Cyprus 
  vatexp.push (/^(CZ)(\d{8,10})(\d{3})?$/);                 //** Czech Republic
  vatexp.push (/^(DE)(\d{9})$/);                            //** Germany 
  vatexp.push (/^(DK)((\d{8}))$/);                          //** Denmark 
  vatexp.push (/^(EE)(\d{9})$/);                            //** Estonia 
  vatexp.push (/^(EL)(\d{8,9})$/);                          //** Greece 
  vatexp.push (/^(ES)([A-Z]\d{8})$/);                       //** Spain (1)
  vatexp.push (/^(ES)(\d{8}[A-Z])$/);                       // Spain (2)
  vatexp.push (/^(ES)([A-Z]\d{7}[A-Z])$/);                  //** Spain (3)
  vatexp.push (/^(EU)(\d{9})$/);                            //** EU-type 
  vatexp.push (/^(FI)(\d{8})$/);                            //** Finland 
  vatexp.push (/^(FR)(\d{11})$/);                           //** France (1)
  vatexp.push (/^(FR)[(A-H)|(J-N)|(P-Z)]\d{10}$/);          // France (2)
  vatexp.push (/^(FR)\d[(A-H)|(J-N)|(P-Z)]\d{9}$/);         // France (3)
  vatexp.push (/^(FR)[(A-H)|(J-N)|(P-Z)]{2}\d{9}$/);        // France (4)
  vatexp.push (/^(GB)?(\d{9})$/);                           //** UK (standard)
  vatexp.push (/^(GB)?(\d{10})$/);                          //** UK (Commercial)
  vatexp.push (/^(GB)?(\d{12})$/);                          //UK (IOM standard)
  vatexp.push (/^(GB)?(\d{13})$/);                          //UK (IOM commercial)
  vatexp.push (/^(GB)?(GD\d{3})$/);                         //** UK (Government)
  vatexp.push (/^(GB)?(HA\d{3})$/);                         //** UK (Health authority)
  vatexp.push (/^(GR)(\d{8,9})$/);                          //** Greece 
  vatexp.push (/^(HU)(\d{8})$/);                            //** Hungary 
  vatexp.push (/^(IE)(\d{7}[A-W])$/);                       //** Ireland (1)
  vatexp.push (/^(IE)([7-9][A-Z\*\+)]\d{5}[A-W])$/);        //** Ireland (2)
  vatexp.push (/^(IT)(\d{11})$/);                           //** Italy 
  vatexp.push (/^(LV)(\d{11})$/);                           //** Latvia 
  vatexp.push (/^(LT)(\d{9}|\d{12})$/);                     //** Lithunia
  vatexp.push (/^(LU)(\d{8})$/);                            //** Luxembourg 
  vatexp.push (/^(MT)(\d{8})$/);                            //** Malta
  vatexp.push (/^(NL)(\d{9})B\d{2}$/);                      //** Netherlands
  vatexp.push (/^(PL)(\d{10})$/);                           //** Poland
  vatexp.push (/^(PT)(\d{9})$/);                            //** Portugal
  vatexp.push (/^(RO)(\d{2,10})$/);                         //** Romania
  vatexp.push (/^(SI)(\d{8})$/);                            //** Slovenia
  vatexp.push (/^(SK)(\d{9}|\d{10})$/);                     // Slovakia Republic
  vatexp.push (/^(SE)(\d{10}\d[1-4])$/);                    //** Sweden

  // Load up the string to check
  var VATNumber = toCheck.toUpperCase();
  
  // Remove spaces from the VAT number to help validation
  var chars = [" ","-",",","."];
  for ( var i=0; i<chars.length; i++) {
    while (VATNumber.indexOf(chars[i])!= -1) {
      VATNumber = VATNumber.slice (0,VATNumber.indexOf(chars[i])) + VATNumber.slice (VATNumber.indexOf(chars[i])+1);
    }
  }

  // Assume we're not going to find a valid VAT number
  var valid = false;                         
  
  // Check the string against the types of VAT numbers
  for (i=0; i<vatexp.length; i++) {
    if (vatexp[i].test(VATNumber)) {
      
      var cCode = RegExp.$1;                             // Isolate country code
      var cNumber = RegExp.$2;                           // Isolate the number
      if (cCode.length == 0) cCode = defCCode;           // Set up default country code
      
      // Now look at the check digits for those countries we know about.
      switch (cCode) {     
        case "AT":
          valid = ATVATCheckDigit (cNumber);
          break;        
        case "BE":
          valid = BEVATCheckDigit (cNumber);
          break;          
        case "BG":
          // The SIMA validation rules are incorrect for Bulgarian numbers.
          //valid = BGVATCheckDigit (cNumber)
          valid = true;
          break;             
        case "CY":
          valid = CYVATCheckDigit (cNumber);
          break;          
        case "CZ":
          valid = CZVATCheckDigit (cNumber);
          break;      
        case "DE":
          valid = DEVATCheckDigit (cNumber);
          break;       
        case "DK":
          valid = DKVATCheckDigit (cNumber);
          break;                 
        case "EE":
          valid = EEVATCheckDigit (cNumber);
          break;             
        case "EL":
          valid = ELVATCheckDigit (cNumber);
          break;       
        case "ES":
          valid = ESVATCheckDigit (cNumber);
          break;       
        case "EU":
          valid = EUVATCheckDigit (cNumber);
          break;       
        case "FI":
          valid = FIVATCheckDigit (cNumber);
          break;        
        case "FR":
          valid = FRVATCheckDigit (cNumber);
          break;         
        case "GB":
          valid = UKVATCheckDigit (cNumber);
          break;                     
        case "GR":
          valid = ELVATCheckDigit (cNumber);
          break;             
        case "HU":
          valid = HUVATCheckDigit (cNumber);
          break;         
        case "IE":
          valid = IEVATCheckDigit (cNumber);
          break;              
        case "IT":
          valid = ITVATCheckDigit (cNumber);
          break;            
        case "LT":
          valid = LTVATCheckDigit (cNumber);
          break;            
        case "LU":
          valid = LUVATCheckDigit (cNumber);
          break;            
        case "LV":
          valid = LVVATCheckDigit (cNumber);
          break;             
        case "MT":
          valid = MTVATCheckDigit (cNumber);
          break;           
        case "NL":
          valid = NLVATCheckDigit (cNumber);
          break;            
        case "PL":      
          valid = PLVATCheckDigit (cNumber);
          break;         
        case "PT":
          valid = PTVATCheckDigit (cNumber);
          break;           
        case "RO":
          valid = ROVATCheckDigit (cNumber);
          break;       
        case "SE":
          valid = SEVATCheckDigit (cNumber);
          break;        
        case "SI":
          valid = SIVATCheckDigit (cNumber);
          break;     
        default:
          valid = true;
      }
            
      // Load new VAT number back into the form element
      if (valid) valid = VATNumber;
      
      // We have found that the number is valid - break from loop
      break;
    }
  }
  
  // Return with either an error or the reformatted VAT number
  return valid;
}

function ATVATCheckDigit (vatnumber) {

  // Checks the check digits of an Austrian VAT number.
  
  var total = 0;
  var multipliers = [1,2,1,2,1,2,1];
  var temp = 0;
  
  // Extract the next digit and multiply by the appropriate multiplier.  
  for (var i = 0; i < 7; i++) {
    temp = Number(vatnumber.charAt(i)) * multipliers[i];
    if (temp > 9)
      total = total + Math.floor(temp/10) + temp%10
    else
      total = total + temp;
  }  
  
  // Establish check digit.
  total = 10 - (total+4) % 10; 
  if (total == 10) total = 0;
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (7,8)) 
    return true
  else 
    return false;
}

function BEVATCheckDigit (vatnumber) {

  // Checks the check digits of a Belgium VAT number.
  
  // First character of 10 digit numbers should be 0
  if (vatnumber.length == 10 && vatnumber.slice(0,1) != "0") return false;
  
  // Nine digit numbers have a 0 inserted at the front.
  if (vatnumber.length == 9) vatnumber = "0" + vatnumber;
  
  // Modulus 97 check on last nine digits
  if (97 - vatnumber.slice (0,8) % 97 == vatnumber.slice (8,10)) 
    return true
  else 
    return false;
}

function BGVATCheckDigit (vatnumber) {

  // Check the check digit of 10 digit Bulgarian VAT numbers.
  if (vatnumber.length != 10) return true;
  var total = 0;
  var multipliers = [4,3,2,7,6,5,4,3,2];
  var temp = 0;
  
  // Extract the next digit and multiply by the appropriate multiplier.  
  for (var i = 0; i < 9; i++) {
    temp = temp + Number(vatnumber.charAt(i)) * multipliers[i];
  }  
  
  // Establish check digit.
  total = 11 - total % 11;
  if (total == 10) total = 0; 
  if (total == 11) total = 1; 
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (9,10)) 
    return true
  else 
    return false;
}

function CYVATCheckDigit (vatnumber) {

  // Checks the check digits of a Cypriot VAT number.
  
  // Extract the next digit and multiply by the counter.
  var total = 0;
  for (var i = 0; i < 8; i++) {
    var temp = Number(vatnumber.charAt(i));
    if (i % 2 == 0) {
      switch (temp) {
        case 0: temp = 1; break;
        case 1: temp = 0; break;
        case 2: temp = 5; break;
        case 3: temp = 7; break;
        case 4: temp = 9; break;
        default: temp = temp*2 + 3;
      }
    }
    total = total + temp;
  }
  
  // Establish check digit using modulus 26, and translate to char. equivalent.
  total = total % 26;
  total = String.fromCharCode(total+65);
  
  // Check to see if the check digit given is correct
  if (total == vatnumber.substr (8,1)) 
    return true 
  else 
    return false;
}

function CZVATCheckDigit (vatnumber) {

  // Checks the check digits of a Czech Republic VAT number.
  
  var total = 0;
  var multipliers = [8,7,6,5,4,3,2];
  
  // Only do check digit validation for standard VAT numbers
  if (vatnumber.length != 8) return true;
  
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 7; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digit.
  total = 11 - total % 11;
  if (total == 10) total = 0; 
  if (total == 11) total = 1; 
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (7,8)) 
    return true
  else 
    return false;
}

function DEVATCheckDigit (vatnumber) {

  // Checks the check digits of a German VAT number.
  
  var product = 10;
  var sum = 0;     
  var checkdigit = 0;                      
  for (var i = 0; i < 8; i++) {
    
    // Extract the next digit and implement perculiar algorithm!.
    sum = (Number(vatnumber.charAt(i)) + product) % 10;
    if (sum == 0) {sum = 10};
    product = (2 * sum) % 11;
  }
  
  // Establish check digit.  
  if (11 - product == 10) {checkdigit = 0} else {checkdigit = 11 - product};
  
  // Compare it with the last two characters of the VAT number. If the same, 
  // then it is a valid check digit.
  if (checkdigit == vatnumber.slice (8,9))
    return true
  else 
    return false;
}

function DKVATCheckDigit (vatnumber) {

  // Checks the check digits of a Danish VAT number.
  
  var total = 0;
  var multipliers = [2,7,6,5,4,3,2,1];
  
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 8; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digit.
  total = total % 11;
  
  // The remainder should be 0 for it to be valid..
  if (total == 0) 
    return true
  else 
    return false;
}

function EEVATCheckDigit (vatnumber) {

  // Checks the check digits of an Estonian VAT number.
  
  var total = 0;
  var multipliers = [3,7,1,3,7,1,3,7];
  
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 8; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digits using modulus 10.
  total = 10 - total % 10;
  if (total == 10) total = 0;
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (8,9))
    return true
  else 
    return false;
}

function ELVATCheckDigit (vatnumber) {

  // Checks the check digits of a Greek VAT number.
  
  var total = 0;
  var multipliers = [256,128,64,32,16,8,4,2];
  
  //eight character numbers should be prefixed with an 0.
  if (vatnumber.length == 8) {vatnumber = "0" + vatnumber};
  
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 8; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digit.
  total = total % 11;
  if (total > 9) {total = 0;};  
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (8,9)) 
    return true
  else 
    return false;
}

function ESVATCheckDigit (vatnumber) {

  // Checks the check digits of a Spanish VAT number.
  
  var total = 0; 
  var temp = 0;
  var multipliers = [2,1,2,1,2,1,2];
  var esexp = new Array ();
  esexp.push (/^[A-H]\d{8}$/);
  esexp.push (/^[N|P|Q|S]\d{7}[A-Z]$/);
  esexp.push (/^[0-9]{8}[A-Z]$/);
  var i = 0;
  
  // With profit companies
  if (esexp[0].test(vatnumber)) {
  
    // Extract the next digit and multiply by the counter.
    for (i = 0; i < 7; i++) {
      temp = Number(vatnumber.charAt(i+1)) * multipliers[i];
      if (temp > 9) 
        total = total + Math.floor(temp/10) + temp%10 
      else 
        total = total + temp;
    }   
    
    // Now calculate the check digit itself. 
    total = 10 - total % 10;
    if (total == 10) {total = 0;}
    
    // Compare it with the last character of the VAT number. If it is the same, 
    // then it's a valid check digit.
    if (total == vatnumber.slice (8,9)) 
      return true
    else 
      return false;
  }
  
  // Non-profit companies
  else if (esexp[1].test(vatnumber)) {
  
    // Extract the next digit and multiply by the counter.
    for (i = 0; i < 7; i++) {
      temp = Number(vatnumber.charAt(i+1)) * multipliers[i];
      if (temp > 9) 
        total = total + Math.floor(temp/10) + temp%10 
      else 
        total = total + temp;
    }    
    
    // Now calculate the check digit itself.
    total = 10 - total % 10;
    total = String.fromCharCode(total+64);
    
    // Compare it with the last character of the VAT number. If it is the same, 
    // then it's a valid check digit.
    if (total == vatnumber.slice (8,9)) 
      return true
    else 
      return false;
  }
	
  // Personal number (NIF)
  else if (esexp[2].test(vatnumber)) {
  	return vatnumber.charAt(8) == 'TRWAGMYFPDXBNJZSQVHLCKE'.charAt(Number(vatnumber.substring(0, 8)) % 23);
  }
	
  else return true;
}

function EUVATCheckDigit (vatnumber) {

  // We know litle about EU numbers apart from the fact that the first 3 digits 
  // represent the country, and that there are nine digits in total.
  return true;
}

function FIVATCheckDigit (vatnumber) {

  // Checks the check digits of a Finnish VAT number.
  
  var total = 0; 
  var multipliers = [7,9,10,5,8,4,2];
  
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 7; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digit.
  total = 11 - total % 11;
  if (total > 9) {total = 0;};  
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (7,8)) 
    return true
  else 
    return false;
}

function FRVATCheckDigit (vatnumber) {

  // Checks the check digits of a French VAT number.
  
  if (!(/^\d{11}$/).test(vatnumber)) return true;
  
  // Extract the last nine digits as an integer.
  var total = vatnumber.substring(2); 
  
  // Establish check digit.
  total = (total*100+12) % 97;
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (0,2)) 
    return true
  else 
    return false;
}

function HUVATCheckDigit (vatnumber) {

  // Checks the check digits of a Hungarian VAT number.
  
  var total = 0;
  var multipliers = [9,7,3,1,9,7,3];
  
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 7; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digit.
  total = 10 - total % 10; 
  if (total == 10) total = 0;
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (7,8)) 
    return true
  else 
    return false;
}

function IEVATCheckDigit (vatnumber) {

  // Checks the check digits of an Irish VAT number.
  
  var total = 0; 
  var multipliers = [8,7,6,5,4,3,2];
  
  // If the code is in the old format, we need to convert it to the new.
  if (/^\d[A-Z\*\+]/.test(vatnumber)) {
    vatnumber = "0" + vatnumber.substring(2,7) + vatnumber.substring(0,1) + vatnumber.substring(7,8);
  }
    
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 7; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digit using modulus 23, and translate to char. equivalent.
  total = total % 23;
  if (total == 0)
    total = "W"
  else
    total = String.fromCharCode(total+64);
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (7,8)) 
    return true
  else 
    return false;
}

function ITVATCheckDigit (vatnumber) {

  // Checks the check digits of an Italian VAT number.
  
  var total = 0;
  var multipliers = [1,2,1,2,1,2,1,2,1,2];
  var temp;
    
  // The last three digits are the issuing office, and cannot exceed more 201
  temp=Number(vatnumber.slice(0,7));
  if (temp==0) return false;
  temp=Number(vatnumber.slice(7,10));
  if ((temp<1) || (temp>201)) return false;
  
  // Extract the next digit and multiply by the appropriate  
  for (var i = 0; i < 10; i++) {
    temp = Number(vatnumber.charAt(i)) * multipliers[i];
    if (temp > 9) 
      total = total + Math.floor(temp/10) + temp%10 
    else 
      total = total + temp;
  }
  
  // Establish check digit.
  total = 10 - total % 10;
  if (total > 9) {total = 0;};  
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (10,11)) 
    return true
  else 
    return false;
}

function LTVATCheckDigit (vatnumber) {

  // Checks the check digits of a Lithuanian VAT number.
  
  // Only do check digit validation for standard VAT numbers
  if (vatnumber.length != 9) return true;
  
  // Extract the next digit and multiply by the counter+1.
  var total = 0;
  for (var i = 0; i < 8; i++) total = total + Number(vatnumber.charAt(i)) * (i+1);
  
  // Can have a double check digit calculation!
  if (total % 11 == 10) {
    var multipliers = [3,4,5,6,7,8,9,1];
    total = 0;
    for (i = 0; i < 8; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  }
  
  // Establish check digit.
  total = total % 11;
  if (total == 10) {total = 0;}; 
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (8,9)) 
    return true
  else 
    return false;
}

function LUVATCheckDigit (vatnumber) {

  // Checks the check digits of a Luxembourg VAT number.
  
  if (vatnumber.slice (0,6) % 89 == vatnumber.slice (6,8)) 
    return true
  else 
    return false;
}

function LVVATCheckDigit (vatnumber) {

  // Checks the check digits of a Latvian VAT number.
  
  // Only check the legal bodies
  if ((/^[0-3]/).test(vatnumber)) return true; 
  
  var total = 0;
  var multipliers = [9,1,4,8,3,10,2,5,7,6];
  
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 10; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digits by getting modulus 11.
  if (total%11 == 4 && vatnumber[0] ==9) total = total - 45;
  if (total%11 == 4) 
    total = 4 - total%11
  else if (total%11 > 4) 
    total = 14 - total%11
  else if (total%11 < 4) 
    total = 3 - total%11;
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (10,11)) 
    return true
  else 
    return false;
}

function MTVATCheckDigit (vatnumber) {

  // Checks the check digits of a Maltese VAT number.
  
  var total = 0;
  var multipliers = [3,4,6,7,8,9];
  
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 6; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digits by getting modulus 37.
  total = 37 - total % 37;
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (6,8) * 1) 
    return true
  else 
    return false;
}

function NLVATCheckDigit (vatnumber) {

  // Checks the check digits of a Dutch VAT number.
  
  var total = 0;                                 // 
  var multipliers = [9,8,7,6,5,4,3,2];
  
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 8; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digits by getting modulus 11.
  total = total % 11;
  if (total > 9) {total = 0;};  
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (8,9)) 
    return true
  else 
    return false;
}

function PLVATCheckDigit (vatnumber) {

  // Checks the check digits of a Polish VAT number.
  
  var total = 0;
  var multipliers = [6,5,7,2,3,4,5,6,7];
  
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 9; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digits subtracting modulus 11 from 11.
  total = total % 11;
  if (total > 9) {total = 0;};
  
  // Compare it with the last character of the VAT number. If it is the same, then it's a valid 
  // check digit.
  if (total == vatnumber.slice (9,10)) 
    return true
  else 
    return false;
}

function PTVATCheckDigit (vatnumber) {

  // Checks the check digits of a Portugese VAT number.
  
  var total = 0;
  var multipliers = [9,8,7,6,5,4,3,2];
  
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 8; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digits subtracting modulus 11 from 11.
  total = 11 - total % 11;
  if (total > 9) {total = 0;};
  
  // Compare it with the last character of the VAT number. If it is the same, then it's a valid 
  // check digit.
  if (total == vatnumber.slice (8,9)) 
    return true
  else 
    return false;
}

function ROVATCheckDigit (vatnumber) {

  // Checks the check digits of a Romanian VAT number.
  
  var multipliers = [7,5,3,2,1,7,5,3,2,1];
  
  // Extract the next digit and multiply by the counter.
  var VATlen = vatnumber.length;
  multipliers = multipliers.slice (10-VATlen);
  var total = 0;
  for (var i = 0; i < vatnumber.length-1; i++) {
    total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  }  
  
  // Establish check digits by getting modulus 11.
  total = (10 * total) % 11;
  if (total == 10) total = 0; 
  
  // Compare it with the last character of the VAT number. If it is the same, then it's a valid 
  // check digit.
  if (total == vatnumber.slice (vatnumber.length-1, vatnumber.length))
    return true
  else 
    return false;
}

function SEVATCheckDigit (vatnumber) {

  // Checks the check digits of a Swedish VAT number.
  
  var total = 0;
  var multipliers = [2,1,2,1,2,1,2,1,2];
  var temp = 0;
  
  // Extract the next digit and multiply by the appropriate multiplier.
  for (var i = 0; i < 9; i++) {
    temp = Number(vatnumber.charAt(i)) * multipliers[i];
    if (temp > 9)
      total = total + Math.floor(temp/10) + temp%10
    else 
      total = total + temp;
  }
  
  // Establish check digits by subtracting mod 10 of total from 10.
  total = 10 - (total % 10); 
  if (total == 10) total = 0;
  
  // Compare it with the 10th character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (9,10)) 
    return true
  else 
    return false;
}

function SKVATCheckDigit (vatnumber) {

  // Checks the check digits of a Slovak VAT number.
  
  var total = 0; 
  var multipliers = [8,7,6,5,4,3,2];
  
  // Extract the next digit and multiply by the counter.
  for (var i = 3; i < 9; i++) {
    total = total + Number(vatnumber.charAt(i)) * multipliers[i-3];
  }  
  
  // Establish check digits by getting modulus 11.
  total = 11 - total % 11;
  if (total > 9) total = total - 10;  
  
  // Compare it with the last character of the VAT number. If it is the same, 
  // then it's a valid check digit.
  if (total == vatnumber.slice (9,10)) 
    return true
  else 
    return false;
}

function SIVATCheckDigit (vatnumber) {

  // Checks the check digits of a Slovenian VAT number.
  
  var total = 0; 
  var multipliers = [8,7,6,5,4,3,2];
  
  // Extract the next digit and multiply by the counter.
  for (var i = 0; i < 7; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
  
  // Establish check digits by subtracting 97 from total until negative.
  total = 11 - total % 11;
  if (total > 9) {total = 0;};  
  
  // Compare the number with the last character of the VAT number. If it is the 
  // same, then it's a valid check digit.
  if (total == vatnumber.slice (7,8)) 
    return true
  else 
    return false;
}

function UKVATCheckDigit (vatnumber) {

  // Checks the check digits of a UK VAT number.
  
  var multipliers = [8,7,6,5,4,3,2];
  
  // Government departments
  if (vatnumber.substr(0,2) == 'GD') {
    if (vatnumber.substr(2,3) < 500) 
      return true 
    else 
      return false;
  }
  
  // Health authorities
  if (vatnumber.substr(0,2) == 'HA') {
    if (vatnumber.substr(2,3) > 499) 
      return true 
    else 
      return false;
  }
  
  // Standard and commercial numbers
  if (vatnumber.length == 9 || vatnumber.length == 10) {
    var total = 0;
    if (vatnumber.length == 10 && vatnumber.slice (9,10) != '3') return false;
    
    // Extract the next digit and multiply by the counter.
    for (var i = 0; i < 7; i++) total = total + Number(vatnumber.charAt(i)) * multipliers[i];
	
    // Old numbers use a simple 97 modulus, but new numbers use an adaptation of that (less
    // 55). Our VAT number could use either system, so we check it against both.
  
    // Establish check digits by subtracting 97 from total until negative.
    var cd = total;
    while (cd > 0) {cd = cd - 97;}    
  
    // Get the absolute value and compare it with the last two characters of the
    // VAT number. If the same, then it is a valid traditional check digit.
    cd = Math.abs(cd);
    if (cd == vatnumber.slice (7,9)) return true;
    
    // Now try the new method by subtracting 55 from the check digit if we can - else add 42
    if (cd >= 55) 
      cd = cd - 55
    else
      cd = cd + 42;
    if (cd == vatnumber.slice (7,9)) 
      return true
    else
      return false;
    
  }
  
  // We don't check 12 and 13 digit UK numbers - not only can we not find any, 
  // but the information found on the format is contradictory.
  
  return true;
}
