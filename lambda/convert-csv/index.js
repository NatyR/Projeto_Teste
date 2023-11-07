const s3Util = require("./s3-util"),
  fs = require("fs"),
  nodemailer = require("nodemailer"),
  path = require("path"),
  convert = require("./convert"),
  os = require("os"),
  SMTP_HOST = process.env.SMTP_HOST,
  SMTP_PORT = process.env.SMTP_PORT,
  SMTP_FROM = process.env.SMTP_FROM,
  SMTP_PWD = process.env.SMTP_PWD,
  SMTP_TO = process.env.SMTP_TO,
  EXTENSION = process.env.EXTENSION,
  OUTPUT_BUCKET = process.env.RESULTS_BUCKET_NAME,
  MIME_TYPE = process.env.MIME_TYPE;
async function send365Email(fileCsv,file, convenio) {
  const from = SMTP_FROM || "atendimento@bullla.com.br";
  const to = SMTP_TO || "felipe@febinfo.com.br";
  let subject = "Novo arquivo de importação";
  const html = `<html><body><p>Novo arquivo para importação</p><p>Convênio: ${convenio}</p></body></html>`;
  const text = `Novo arquivo para importação. Convênio: ${convenio}`;
  const parts = file.split("/");
  const filename = parts[parts.length - 1];
  const name_ext = filename.split(".");
  const fileNameParts = name_ext[0].split("_");
  const tipo = fileNameParts[fileNameParts.length - 1];
  switch (tipo) {
    case "INDIVIDUAL":
      subject = "Cadastro individual";
      break;
    case "BLOQUEIO":
      subject = "Bloquear cartão";
      break;
    case "DESBLOQUEIO":
      subject = "Desbloqueio de cartão";
      break;
    case "SEGUNDAVIA":
      subject = "2ª via de cartão";
      break;
    case "REMESSA":
      subject = "Cadastro em lote";
      break;
    case "LIMITE":
      subject = "Alteração de limite";
      break;
    default:
      break;
  }



  try {
    //var template = fs.readFileSync('template.html',{encoding:'utf-8'});
    const transportOptions = {
      host: SMTP_HOST || "smtp.office365.com",
      port: SMTP_PORT || "587",
      auth: { user: from, pass: SMTP_PWD || "8Mcs*BZYbR4x" },
      secureConnection: true,
      tls: { ciphers: "SSLv3" },
    };

    const mailTransport = nodemailer.createTransport(transportOptions);

    await mailTransport.sendMail({
      from,
      to,
      replyTo: from,
      subject,
      html,
      text,
      attachments: [
        {
          filename: filename,
          content: fs.createReadStream(file),
        },
        {
          filename: filename.replace('.unk','.csv'),
          content: fs.createReadStream(fileCsv),
        },
      ],
    });
  } catch (err) {
    console.error(`send365Email: An error occurred:`, err);
  }
}
exports.handler = function (eventObject, context) {
  const eventRecord = eventObject.Records && eventObject.Records[0],
    inputBucket = eventRecord.s3.bucket.name,
    key = eventRecord.s3.object.key,
    id = context.awsRequestId,
    resultKey = key
      .replace("accounts/import/", "accounts/converted/")
      .replace(/\.[^.]+$/, EXTENSION);
  const partsName = resultKey.split("/");
  const baseName = partsName[partsName.length - 1];
  const tempPath = path.join(os.tmpdir(), id),
    convertedPath = path.join(os.tmpdir(), baseName);
  const parts = key.split("/");
  const fileName = parts[parts.length - 1];
  const fileNameParts = fileName.split("_");
  const convenio = fileNameParts[0];
  console.log("converting", inputBucket, key, "using", tempPath);
  return s3Util
    .downloadFileFromS3(inputBucket, key, tempPath)
    .then(() => convert(tempPath, convertedPath, convenio))
    .then(() =>
      s3Util.uploadFileToS3(OUTPUT_BUCKET, resultKey, convertedPath, MIME_TYPE)
    )
    .then(() => send365Email(tempPath,convertedPath, convenio));
};
