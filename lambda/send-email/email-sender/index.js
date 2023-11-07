var aws = require("aws-sdk");
var fs = require("fs");
const nodemailer = require("nodemailer");
var s3 = new aws.S3();
let response;
//{"from":"atendimento@bullla.com.br",
//"to":"felipe@febinfo.com.br",
//"subject":"Teste",
//"html":null,
//"text":"Teste"}

function getS3File(bucket, key) {
  return new Promise(function (resolve, reject) {
    s3.getObject(
      {
        Bucket: bucket,
        Key: key,
      },
      function (err, data) {
        if (err) return reject(err);
        else return resolve(data);
      }
    );
  });
}

/**
 *
 * Event doc: https://docs.aws.amazon.com/apigateway/latest/developerguide/set-up-lambda-proxy-integrations.html#api-gateway-simple-proxy-for-lambda-input-format
 * @param {Object} event - API Gateway Lambda Proxy Input Format
 *
 * Context doc: https://docs.aws.amazon.com/lambda/latest/dg/nodejs-prog-model-context.html
 * @param {Object} context
 *
 * Return doc: https://docs.aws.amazon.com/apigateway/latest/developerguide/set-up-lambda-proxy-integrations.html
 * @returns {Object} object - API Gateway Lambda Proxy Output Format
 *
 */
exports.handler = async (event, context) => {
  try {
    var messages = [];
    if (!event.rmqMessagesByQueue) {
      response = {
        statusCode: 404,
        body: JSON.stringify({
          message: "Invalid event data",
          // location: ret.data.trim()
        }),
      };
    } else {
      messages.push("Data received from event source:");
      console.log("Data received from event source:");
      for (var queue in event.rmqMessagesByQueue) {
        const messageCnt = event.rmqMessagesByQueue[queue].length;
        messages.push(
          `Total messages received from event source: ${messageCnt}`
        );
        console.log(`Total messages received from event source: ${messageCnt}`);
        for (var msg in event.rmqMessagesByQueue[queue]) {
          data = Buffer.from(
            event.rmqMessagesByQueue[queue][msg].data,
            "base64"
          ).toString("utf8");
          messages.push(data);
          console.log(data);
          const obj = JSON.parse(data);
          await send365Email(obj);
        }
      }
      response = {
        statusCode: 200,
        body: JSON.stringify({
          message: messages.join("||"),
          // location: ret.data.trim()
        }),
      };
    }
  } catch (err) {
    console.log(err);
    return err;
  }

  return response;
};
async function send365Email(message) {
  try {
    const { from, to, subject, html, text, attachments } = message;
    var template = fs.readFileSync("template.html", { encoding: "utf-8" });
    const transportOptions = {
      host: "smtp.office365.com",
      port: "587",
      auth: { user: from, pass: "8Mcs*BZYbR4x" },
      secureConnection: true,
      tls: { ciphers: "SSLv3" },
    };

    const mailTransport = nodemailer.createTransport(transportOptions);
    var mailOptions = {
      from,
      to,
      replyTo: from,
      subject,
      html: template.replace("{{content}}", html),
      text,
    };
    if(attachments && attachments.length > 0){
      mailOptions.attachments = [];
      for(var i = 0; i < attachments.length; i++){
        const attachment = await getS3File('bullla-empresas', attachments[i].key);
        mailOptions.attachments.push({
          filename: attachments[i].fileName,
          content: attachment.Body
        });
      }
    }
    await mailTransport.sendMail(mailOptions);
  } catch (err) {
    console.error(`send365Email: An error occurred:`, err);
  }
}
