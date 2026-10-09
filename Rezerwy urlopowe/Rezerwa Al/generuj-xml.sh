#!/usr/bin/env bash
# Buduje "Rezerwa urlopowa.dbinit.xml" (import wg rekordów, dbmgr importxml) z plików kodu w tym katalogu:
#   "Rezerwa urlopowa"                  - algorytm elementu (wspólny dla rezerwy i budżetu)
#   "Cecha widoku rezerwy (wzorzec)"    - wzorzec cechy kolumny widoku
#   "Cecha widoku rezerwy MPK"          - cecha MPK
# GUID-y są stałe - ponowny import aktualizuje te same rekordy.
set -euo pipefail
cd "$(dirname "$0")"
OUT="Rezerwa urlopowa.dbinit.xml"
ALG="$(cat 'Rezerwa urlopowa')"
WZ="$(cat 'Cecha widoku rezerwy (wzorzec)')"

element() { # guid nazwa skrot
cat <<EOF
  <DefinicjaElementu guid="$1">
    <Nazwa>$2</Nazwa>
    <Skrot>$3</Skrot>
    <Blokada>False</Blokada>
    <Zatrudnienie>Etat</Zatrudnienie>
    <RodzajZrodla>DodatekAutomatyczny</RodzajZrodla>
    <Kolejnosc>200</Kolejnosc>
    <GenerujZerowy>False</GenerujZerowy>
    <Korygowany>False</Korygowany>
    <DoWyplaty>False</DoWyplaty>
    <RodzajNaliczania>TylkoPlanowane</RodzajNaliczania>
    <OkresNaliczania>
      <Typ>Każda</Typ>
      <Naliczanie>PłatnaZDołu</Naliczanie>
    </OkresNaliczania>
    <Algorytm>
      <Priorytet>200</Priorytet>
      <ZapisObliczen>
        <Nazwa>Rezerwa urlopowa</Nazwa>
      </ZapisObliczen>
      <Typ>EdytorAlgorytmu</Typ>
      <ElPodstawa1>Podstawa 1 (śr. 3 mies.)</ElPodstawa1>
      <ElPodstawa2>Podstawa 2 (standard)</ElPodstawa2>
      <ElPodstawa3>Urlop zaległy (dni)</ElPodstawa3>
      <ElPodstawa4>$4</ElPodstawa4>
      <ElPodstawa5>$5</ElPodstawa5>
      <ElCzas>Godziny rezerwy</ElCzas>
      <Edytor>
        <Tekst><![CDATA[
$ALG
]]></Tekst>
      </Edytor>
    </Algorytm>
    <Deklaracje>
      <Spoleczne>
        <Typ>Naliczać</Typ>
      </Spoleczne>
      <Zdrowotne>
        <Typ>Naliczać</Typ>
      </Zdrowotne>
      <Zaliczka>
        <Typ>NieNaliczać</Typ>
      </Zaliczka>
    </Deklaracje>
    <Nieobecnosci>
      <Urlop>
        <Typ>NieWliczać</Typ>
      </Urlop>
      <Ekwiwalent>
        <Typ>NieWliczać</Typ>
      </Ekwiwalent>
    </Nieobecnosci>
  </DefinicjaElementu>
EOF
}

plan() { # guid symbol nazwa guidElementu
cat <<EOF
  <DefinicjaPlanowanejListyPłac guid="$1">
    <Symbol>$2</Symbol>
    <Nazwa>$3</Nazwa>
    <Blokada>False</Blokada>
    <!-- import wg rekordów nie uzupełnia numeracji ani ewidencji (GUI robi to samo) - bez wzoru
         każda planowana lista dostaje numer "*" i druga lista łamie klucz Numer.WgNumeruDokumentu -->
    <Numeracja>
      <Wzor>Definicja.Symbol/Data.Year:4/Data.Month:2/*</Wzor>
    </Numeracja>
    <DefinicjaED>00000000-0007-0005-0004-000000000000</DefinicjaED>
    <Element>$4</Element>
    <Algorytm><![CDATA[
public override SourceFilterDelegate FiltrNaliczania {
    get { return new SourceFilterDelegate(SourceFilter); }
}

bool SourceFilter(object sender, SourceFilterArgs args) {
    WypElement element = args.Element;
    FromTo okres = args.Okres;
    return true;
}
]]></Algorytm>
  </DefinicjaPlanowanejListyPłac>
EOF
}

cecha() { # guid nazwa element wyrazenie opis
  local metoda kod
  metoda="Feature_${2// /_}"
  kod="${WZ//__METODA__/$metoda}"
  kod="${kod//__ELEMENT__/$3}"
  kod="${kod//__WYRAZENIE__/$4}"
  kod="${kod//__TYP__/decimal}"
cat <<EOF
  <FeatureDefinition guid="$1">
    <TableName>Pracownicy</TableName>
    <Name>$2</Name>
    <Category>Rezerwa urlopowa</Category>
    <Description>$5</Description>
    <TypeNumber>Decimal</TypeNumber>
    <Precision>2</Precision>
    <Algorithm>GetArgs</Algorithm>
    <Code><![CDATA[
$kod
]]></Code>
  </FeatureDefinition>
EOF
}

R="Rezerwa urlopowa"
B="Budżet rezerwy urlopowej"
{
echo '<?xml version="1.0" encoding="utf-8"?>'
echo '<!-- WYGENEROWANE przez generuj-xml.sh - nie edytować ręcznie. Opis: Rezerwa urlopowa.md -->'
echo '<session xmlns="http://www.soneta.pl/schema/business">'
element 0d94d59b-a616-4833-8251-55857735b070 "$R" "Rez.urlop." "Urlop bieżący prop. (dni)" "Urlop wykorzystany (dni)"
element d3e398ac-8c59-4992-95f8-5d348839b194 "$B" "Bud.rez.url." "Limit na rok nast. (dni)" "Wykorzystany (dni)"
plan dc76af63-a30b-4292-8140-3bb0651cc013 "REZURL" "Rezerwa urlopowa" 0d94d59b-a616-4833-8251-55857735b070
plan f7985482-87b0-431b-a64a-3a4a06910e91 "BUDREZURL" "Budżet rezerwy urlopowej" d3e398ac-8c59-4992-95f8-5d348839b194

cat <<EOF
  <FeatureDefinition guid="42b41ec7-094b-432c-b3bf-e5a42d123e5e">
    <TableName>Pracownicy</TableName>
    <Name>Rezerwa MPK</Name>
    <Category>Rezerwa urlopowa</Category>
    <Description>Centrum kosztów z wydziału pracownika</Description>
    <TypeNumber>String</TypeNumber>
    <Algorithm>GetArgs</Algorithm>
    <Code><![CDATA[
$(cat 'Cecha widoku rezerwy MPK')
]]></Code>
  </FeatureDefinition>
EOF
cecha 74672aea-f02b-41df-a16a-3b2544fccd39 "Rezerwa urlop zaległy"      "$R" "(decimal)e.Podstawa3.Value" "Urlop zaległy (dni)"
cecha e65c8bb2-d14c-4bb6-b984-9cb80171d62e "Rezerwa urlop bieżący"      "$R" "(decimal)e.Podstawa4.Value" "Urlop bieżący proporcjonalny (dni)"
cecha 1804550f-d0d4-4755-b85e-b8191e060181 "Rezerwa urlop wykorzystany" "$R" "(decimal)e.Podstawa5.Value" "Urlop wykorzystany do końca miesiąca (dni)"
cecha f7a0acc9-3dc3-4402-bed9-98978e0ce08a "Rezerwa godziny"            "$R" "(decimal)e.Czas.TotalHours" "Godziny rezerwy"
cecha acc59bd7-e26a-4250-9a17-85ff21352d1e "Rezerwa podstawa 1"         "$R" "(decimal)e.Podstawa1.Value" "Podstawa 1 - średnia z 3 miesięcy"
cecha af64f82d-4f33-4f4e-a65e-9a9422a764d4 "Rezerwa podstawa 2"         "$R" "(decimal)e.Podstawa2.Value" "Podstawa 2 - standard enova"
cecha 9d147894-6f79-4506-87ba-1d0c5c2c2c6d "Rezerwa kwota"              "$R" "(decimal)e.Wartosc" "Kwota rezerwy"
cecha 1bdb5d86-6ee5-4560-b2c8-cfb354cf478c "Rezerwa narzuty"            "$R" "(decimal)e.Narzuty" "Narzuty pracodawcy od rezerwy"
cecha 701e0952-965a-4b6a-ba40-01c2a107ef3d "Budżet urlop zaległy"       "$B" "(decimal)e.Podstawa3.Value" "Urlop zaległy na 01.01 roku następnego (dni)"
cecha d431e061-0917-4869-893e-ef733b449670 "Budżet urlop należny"       "$B" "(decimal)e.Podstawa4.Value" "Limit urlopu na rok następny (dni)"
cecha 344e5905-794e-49e5-acae-6a8b151c25f4 "Budżet godziny"             "$B" "(decimal)e.Czas.TotalHours" "Godziny budżetu"
cecha 6e64397a-72db-4a9e-878f-7ecc423ed2a3 "Budżet podstawa 1"          "$B" "(decimal)e.Podstawa1.Value" "Podstawa 1 - średnia z 3 miesięcy"
cecha 133c3199-d5ff-4e2c-8e3a-c3af356f50a7 "Budżet podstawa 2"          "$B" "(decimal)e.Podstawa2.Value" "Podstawa 2 - standard enova"
cecha 282d09b8-2bd3-4d8e-b59e-ab35eae39e3f "Budżet kwota"               "$B" "(decimal)e.Wartosc" "Kwota budżetu rezerwy"
cecha 3d1e09c5-2998-43dd-8946-a279ef3d26ba "Budżet narzuty"             "$B" "(decimal)e.Narzuty" "Narzuty pracodawcy od budżetu"
echo '</session>'
} > "$OUT"
echo "Zapisano: $OUT"
