'use strict';
const csv = require('fast-csv');
const fs = require('fs');
const sanitizer = (str) => {
	if (str){
		//remove accents
		str = str.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
		//remove special characters
		str = str.replace(/[^a-zA-Z0-9\s]/g, '');
		//remove spaces
		//str = str.replace(/\s/g, '');
		//convert to upper case
		str = str.toUpperCase();
		return str;
	} else {
		return "";
	}
};
const fillText = (str, length) => {
  //trim str
  str = str.trim();
  if (str.length > length) {
    return str.substring(0, length);
  }
  return str + ' '.repeat(length - str.length);
};
const fillNumber = (str, length, left = false) => {
  //trim str
  str = str.toString();
  str = str.replace(/[^0-9]/g, '');
  str = str.trim();
  if (str.length > length) {
    return str.substring(0, length);
  }
  if (left) {
    return str + ' '.repeat(length - str.length);
  } else {
    return '0'.repeat(length - str.length) + str;
  }
};
const getPhone = (str, length = 9) => {
  //split by ')' and space
  var arr = str.split(') ');
  if (arr.length > 1) {
    str = arr[1];
  } else {
    str = arr[0];
  }
  str = str.replace(/[^0-9]/g, '');
  if (str.length > length) {
    return str.substring(-length);
  }
  return fillText(str, length);
};
const getFullPhone = (str, length = 15) => {
  //split by ')' and space
  str = str.replace(/[^0-9]/g, '');
  if (str.length > length) {
    return str.substring(-length);
  }
  return fillText(str, length);
};
const getDate = (str, def = '') => {
  str = str.toString();
  str = str.trim();
  if (def.length > 0 && str.length == 0) {
    str = def;
  }
  var arr = str.split('/');
  if (arr.length > 1) {
    str = arr.join('');
  }
  return fillNumber(str, 8);
};
const getDecimal = (str, integer = 8, decimals = 2) => {
  str = str.toString();
  str = str.trim();
  //if contains '.' and ',' remove '.' and replace ',' by '.'
  if (str.indexOf('.') > -1 && str.indexOf(',') > -1) {
    str = str.replace('.', '');
    str = str.replace(',', '.');
  }
  if (str.indexOf(',') > -1) {
    str = str.replace(',', '.');
  }
  var arr = str.split('.');
  var dec = arr[1] || '0';
  dec = dec + '0'.repeat(decimals - dec.length);
  return fillText(arr[0] + '.' + dec, integer + decimals);
};
const currentDate = () => {
  var today = new Date();
  var dd = today.getDate();
  var mm = today.getMonth() + 1; //January is 0!
  var yyyy = today.getFullYear();
  if (dd < 10) {
    dd = '0' + dd;
  }
  if (mm < 10) {
    mm = '0' + mm;
  }
  return dd + '/' + mm + '/' + yyyy;
};

module.exports = function convert(inFile, outFile, convenio) {
  return new Promise((resolve, reject) => {
    const stream = fs.createReadStream(inFile);
    const rows = [];
    csv
      .parseStream(stream, { headers: true, delimiter: ';', trim: true })
      .on('data', (data) => rows.push(data))
      .on('end', () => {
        console.log(rows);
        var output = fs.createWriteStream(outFile);
        output.once('open', function (fd) {
          rows.forEach(function (row) {
            var line = '';
            line += fillText('1', 1);
            line += fillText('', 7);
            line += fillText(sanitizer(row.Nome || ''), 40);
            line += fillText('', 25);
            line += fillText(sanitizer(row.Endereco || ''), 30);
            line += fillText(sanitizer(row.Numero || ''), 10);
            line += fillText('', 6);
            line += fillText(sanitizer(row.Bairro || ''), 20);
            line += fillText(sanitizer(row.Cidade || ''), 20);
            line += fillText(sanitizer(row.UF || ''), 2);
            line += getFullPhone(row.Celular || '', 19);
            line += getDate(row['Data Nascimento'] || '');
            line += fillText('', 44);
            line += getDecimal(row.Limite || '0', 8, 2);
            line += getDate(row['Data Admissao'] || '', currentDate());
            line += fillText('', 8);
            line += fillText(row['Matricula'] || '', 11);
            line += fillText('', 4);
            line += fillText('', 6); //Setor
            line += fillText('', 4); //Filial
            line += fillNumber(convenio, 6) + '  ';
            line += fillNumber(row.Movimento || '4', 2); //4 - emissao, 5 - segunda via, 7 - Bloqueio, 8 - Desbloqueio
            line += fillText('', 8); //Data demissão
            line += fillText('', 7);
            line += fillNumber(convenio, 6) + '  ';
            line += fillNumber(row.CPF || '', 11);
            line += fillText('', 85);
            line += fillNumber(0, 2); //motivo segunda via
            line += fillNumber(1, 2); //codigo de grupo de limite
            line += fillText('', 16); //numero do cartao
            line += fillText(row.Email || '', 50); //email
            line += fillText('', 23);
            line += fillNumber(row.CNPJ || '', 14, true); // CNPJ
            line += fillText(row['Codigo Filial'] || '', 16); // Código da Filial
            line += fillText(sanitizer(row['Nome Filial'] || ''), 64); // Nome da Filial
            line += fillText('', 1);
            line += fillText(row['Codigo Centro de Custo'] || '', 16); // Código do Centro de Custo
            line += fillText(sanitizer(row['Nome Centro de Custo'] || ''), 40); // Centro de Custo
            line += fillText(row.Complemento || '', 24); // Complemento
            line += fillNumber(row.CEP || '', 8); // CEP
            line += fillNumber(row['Unidade Entrega'] || '', 10,true); // Unidade de entrega
            line += fillText(sanitizer(row['Entrega Colaborador']) || 'N', 1); // Entrega no endereco do colaborador
            line += fillText('', 5);
            line += fillText(sanitizer(row['Sexo'] || ''), 1); // Sexo
            line += fillText('', 2);
            line += fillText(sanitizer(row['Nacionalidade'] || 'BRASILEIRA'), 50); // Nacionalidade
            line += fillText('', 2);
            line += fillText(sanitizer(row['Cargo'] || ''), 100); // Cargo
            line += fillText('', 3);
            line += fillText(sanitizer(row['Nome Mae'] || ''), 50); // Nome da Mãe
            line += fillText('', 2);
            line += fillText(sanitizer(row['Nome Pai'] || ''), 40); // Nome do Pai
            line += fillText('', 2);
            line += getFullPhone(row.Celular || '', 15);
            line += fillText('', 2);
            line += fillNumber(row.RG || '', 15, true); // RG  
            line += fillText('', 1);
            line += fillText(sanitizer(row.Orgao || ''), 10); // Orgao Emissor
            line += fillText('', 3);
            line += fillNumber('', 8,true); // Data Retorno

            output.write(line + '\n');
          });
          output.end();
          resolve();
        });
      });
  });
};
