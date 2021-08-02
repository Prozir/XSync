using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Configuration;
using Microsoft.Owin.Host.SystemWeb;

namespace XSync.Controllers
{
    public class XDBController : ApiController
    {

        private static string connString = ConfigurationManager.AppSettings.Get("connStr");
        private static string apisecret = ConfigurationManager.AppSettings.Get("apiSecret");
        private static string apiissuer = ConfigurationManager.AppSettings.Get("tokenIssuer");
        SqlConnection con = new SqlConnection(connString);

        [HttpGet]
        public Object GetToken() //get the user token
        {
            string key = apisecret;
            var issuer = apiissuer;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var permClaims = new List<Claim>();
            permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            permClaims.Add(new Claim("valid", "1"));
            permClaims.Add(new Claim("userid", "1"));
            permClaims.Add(new Claim("name", "PetPoojaPOS"));

            var token = new JwtSecurityToken(issuer,
                            issuer,
                            permClaims,
                            expires: DateTime.Now.AddDays(7),
                            signingCredentials: credentials);
            var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
            return new { data = jwt_token };
        }



        // POST: api/XDB/GetUserValues - to get all user values if authentication is successfull
        [Authorize]
        [HttpPost]
        public string GetPurchase()
        {            
                SqlDataAdapter da = new SqlDataAdapter("select * from PurchaseDetails", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    return JsonConvert.SerializeObject(dt);
                }
                else
                {
                    return "Oops! No data available";
                }
        }

        // GET: api/XDB/5
       // public string Get(int id)
       // {
       //     return "value";
       // }

        // POST: api/XDB/InsertUserValues -- to insert values to db if authentication is successfull
       // [Authorize]
        [HttpPost]
        public string InsertPurchase([FromBody] JObject data)//string value)
        {
            /* string jsonstr = "["+data.ToString(Formatting.None)+"]";
             JArray array = JArray.Parse(jsonstr);
             string rootKey = "";
             foreach (JObject content in array.Children<JObject>())
             {
                 foreach (JProperty prop in content.Properties())
                 {                        
                     rootKey = prop.Name;
                 }
             }
             JObject mainobj= (JObject)data[rootKey];
             JObject propertiesobj = (JObject)mainobj["properties"];
            */
            JObject propertiesobj = (JObject)data["properties"];

                string[] ColumnNames = new string[] { "RestaurantName","RestaurantAddress", "RestaurantContact","CustomerName", "CustomerAddress",
                "CustomerPhone","OrderId","OrderDeliveryCharge","OrderType","OrderNoOfPerson","OrderDiscountTotal","OrderTaxTotal","OrderRoundOff",
                "OrderCoreTotal","OrderTotal","OrderCreatedOn","OrderFrom","OrderSubOrderType"};

                //restaurant details
                string resName = (string)propertiesobj.SelectToken("Restaurant.res_name");
                string resAddress = (string)propertiesobj.SelectToken("Restaurant.address");
                string resContactInfo = (string)propertiesobj.SelectToken("Restaurant.contact_information");
                string resID = (string)propertiesobj.SelectToken("Restaurant.RestID");

                //Customer Details
                string custName = (string)propertiesobj.SelectToken("Customer.name");
                string custAddress = (string)propertiesobj.SelectToken("Customer.address");
                string custPhone = (string)propertiesobj.SelectToken("Customer.phone");

                //order details
                string odrID = (string)propertiesobj.SelectToken("Order.orderID");
                string odrDeliveryChrg = (string)propertiesobj.SelectToken("Order.delivery_charges");
                string odrType = (string)propertiesobj.SelectToken("Order.order_type");
                string odrPaymentType = (string)propertiesobj.SelectToken("Order.payment_type");
                string odrTblNo = (string)propertiesobj.SelectToken("Order.table_no");
                string odrNoOfPerson = (string)propertiesobj.SelectToken("Order.no_of_persons");
                string odrDiscountTotal = (string)propertiesobj.SelectToken("Order.discount_total");
                string odrTaxTotal = (string)propertiesobj.SelectToken("Order.tax_total");
                string odrRndOff = (string)propertiesobj.SelectToken("Order.round_off");
                string odrCoreTotal = (string)propertiesobj.SelectToken("Order.core_total");
                string odrTotal = (string)propertiesobj.SelectToken("Order.total");
                string odrCreatedOn = (string)propertiesobj.SelectToken("Order.created_on");
                string odrFrom = (string)propertiesobj.SelectToken("Order.order_from");
                string odrSubOdrType = (string)propertiesobj.SelectToken("Order.sub_order_type");

                string[] ColumnValues = new string[] { resName, resAddress, resContactInfo, custName, custAddress, custPhone, odrID, odrDeliveryChrg,
                odrType,odrNoOfPerson,odrDiscountTotal,odrTaxTotal,odrRndOff,odrCoreTotal,odrTotal,odrCreatedOn,odrFrom,odrSubOdrType};

                //string insertStr = "Insert into PurchaseDetails(";
                StringBuilder insertStr = new System.Text.StringBuilder("Insert into PurchaseDetails(");
                int cntA = 0;
                int cntB = 0;
                foreach (string colname in ColumnNames)
                {
                    cntA++;
                    if (cntA < ColumnNames.Length)
                    {
                        insertStr.Append(colname + ",");
                    }
                    else if(cntA == ColumnNames.Length)
                    insertStr.Append(colname + ") VALUES(");
                }
                foreach (string colval in ColumnValues)
                {
                    cntB++;
                    if (cntB < ColumnValues.Length)
                    {
                        insertStr.Append("('"+colval.Replace("'","''")+"')" + ",");
                    }
                    else if (cntB == ColumnValues.Length)
                        insertStr.Append("('" + colval.Replace("'", "''") + "')" + ")");
                }

                //SqlCommand cmd = new SqlCommand("Insert into MyTable(PersonID,LastName,City) VALUES(" + personid + ",'" + lastname + "'," + "'" + city + "')", con);
                SqlCommand cmd = new SqlCommand(insertStr.ToString(), con);
                con.Open();
                int i = cmd.ExecuteNonQuery();
                con.Close();
                if (i == 1)
                {
                    return "Record inserted successfully";
                }
                else
                {
                    return "A runtime error is encountered.";
                }

        }

        // PUT: api/XDB/5
       // public void Put(int id, [FromBody]string value)
        //{
       // }

        // DELETE: api/XDB/5
       // public void Delete(int id)
       // {
       // }

    }
}
