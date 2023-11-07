ALTER TABLE PORTALRH.T_SOLICITACAO ADD GRUPO_ID NUMBER(38,0);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD NOME_GRUPO VARCHAR2(100);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD NOME_CONVENIO VARCHAR2(100);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD CNPJ_CONVENIO VARCHAR2(20);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD EMAIL VARCHAR2(250);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD TELEFONE VARCHAR2(50);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD RG VARCHAR2(20);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD DATA_NASCIMENTO DATE;
ALTER TABLE PORTALRH.T_SOLICITACAO ADD DATA_ADMISSAO DATE;
ALTER TABLE PORTALRH.T_SOLICITACAO ADD CEP VARCHAR2(10);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD ENDERECO VARCHAR2(250);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD NUMERO VARCHAR2(20);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD COMPLEMENTO VARCHAR2(50);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD BAIRRO VARCHAR2(70);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD CIDADE VARCHAR2(70);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD UF VARCHAR2(2);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD CENTROCUSTO_ID NUMBER(38,0);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD NOME_CENTROCUSTO VARCHAR2(100);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD FILIAL_ID NUMBER(38,0);
ALTER TABLE PORTALRH.T_SOLICITACAO ADD NOME_FILIAL VARCHAR2(100);
ALTER TABLE PORTALRH.T_USUARIO ADD DATA_CADASTRO DATE;
ALTER TABLE PORTALRH.T_USUARIO ADD USUARIO_CADASTRO_ID NUMBER(38,0);
ALTER TABLE PORTALRH.T_SOLICITACAO  MODIFY OBSERVACAO VARCHAR2(4000);
ALTER TABLE PORTALRH.T_SOLICITACAO  MODIFY STATUS_CARTAO CHAR(1);
